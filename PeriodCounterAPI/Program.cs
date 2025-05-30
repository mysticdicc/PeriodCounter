using Microsoft.EntityFrameworkCore;
using PeriodCounterAPI.Data;
using Newtonsoft.Json;
using PeriodLib;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;

namespace PeriodCounterAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContextFactory<PeriodDb>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SQL"))
            );

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://securetoken.google.com/p-tracker-f9e4d";
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "https://securetoken.google.com/p-tracker-f9e4d",
                        ValidateAudience = true,
                        ValidAudience = "p-tracker-f9e4d",
                        ValidateLifetime = true
                    };
                });

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "p-tracker-f9e4d-e00404272f.json")),
            });

            if (OperatingSystem.IsWindows())
            {
                const string eventSource = "PeriodAPI Service";
                const string logName = "Period API Logs";

                // EventLog requires admin rights to create a new source
                if (!EventLog.SourceExists(eventSource))
                {
                    EventLog.CreateEventSource(eventSource, logName);
                }

                builder.Logging.ClearProviders();

                builder.Logging.AddEventLog(new EventLogSettings
                {
                    SourceName = eventSource,
                    LogName = logName
                });

                builder.Logging.AddFilter<EventLogLoggerProvider>(eventSource, LogLevel.Information);
            }

            builder.Services.AddAuthorization();
            builder.Services.AddHostedService<NotificationService>();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/get/all",
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            (HttpContext httpContext, IDbContextFactory<PeriodDb> _PeriodDb) =>
            {
                try
                {
                    using var db = _PeriodDb.CreateDbContext();
                    var idClaim = httpContext.User.Claims.First(x => x.Type == "user_id");
                    var result = db.StartTimes.Where(x => x.UserId == idClaim.Value).OrderByDescending(x => x.StartTime).ToList();
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return Results.Problem(ex.Message);
                }
            });

            app.MapGet("/get/lastsubmitdate",
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            async (HttpContext httpContext, IDbContextFactory<PeriodDb> _PeriodDb) =>
            {
                Console.WriteLine("API get poll");

                try
                {
                    using var db = _PeriodDb.CreateDbContext();
                    var idClaim = httpContext.User.Claims.First(x => x.Type == "user_id");
                    var result = await db.StartTimes.Where(x => x.UserId == idClaim.Value).OrderByDescending(x => x.StartTime).FirstOrDefaultAsync();
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("GetLastSubmit");

            app.MapPost("/post/newdate",
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            async (HttpContext HttpContent, IDbContextFactory<PeriodDb> _PeriodDb, PeriodStartTime periodStartTime) =>
            {
                Console.WriteLine("API submit poll");

                try
                {
                    using var db = _PeriodDb.CreateDbContext();
                    db.Add(periodStartTime);
                    await db.SaveChangesAsync();
                    return Results.Ok(periodStartTime);
                } 
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("PostNew");

            app.MapPost("/post/newdeviceregistration",
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            async (HttpContext httpContent, IDbContextFactory<PeriodDb> _PeriodDb, DeviceRegistration deviceReg) =>
            {
                Console.WriteLine("New device registration");

                try
                {
                    using var db = _PeriodDb.CreateDbContext();
                    var idClaim = httpContent.User.Claims.First(x => x.Type == "user_id");
                    var deviceDb = db.DeviceRegistrations.Find(idClaim.Value);

                    if (null == deviceDb)
                    {
                        deviceReg.UserId = idClaim.Value;
                        db.Add(deviceReg);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        if (deviceDb.Fcm != deviceReg.Fcm)
                        {
                            deviceDb.Fcm = deviceReg.Fcm;
                            await db.SaveChangesAsync();
                        }
                    }

                    return Results.Ok(deviceReg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return Results.Problem(ex.Message);
                }
            })
            .WithName("DeviceRegistration");

            app.MapPost("/delete/starttime",
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            async (HttpContext httpContent, IDbContextFactory<PeriodDb> _PeriodDb, PeriodStartTime startTime) =>
            {
                try
                {
                    using var db = _PeriodDb.CreateDbContext();
                    var idClaim = httpContent.User.Claims.First(x => x.Type == "user_id");
                    var dbStartTime = db.StartTimes.Find(startTime.Id);

                    if (null != dbStartTime)
                    {
                        if (dbStartTime.UserId == idClaim.Value)
                        {
                            db.Remove(dbStartTime);
                            await db.SaveChangesAsync();
                            return Results.Ok(startTime.Id);
                        }
                        else
                        {
                            return Results.Problem("No authority to delete");
                        }
                    }
                    else
                    {
                        return Results.Problem("Entry not found");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return Results.Problem(ex.Message);
                }
            });

            app.MapPost("/delete/many/starttime",
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            async (HttpContext httpContent, IDbContextFactory<PeriodDb> _PeriodDb, List<PeriodStartTime> startTimes) =>
            {
                try
                {
                    using var db = _PeriodDb.CreateDbContext();
                    var idClaim = httpContent.User.Claims.First(x => x.Type == "user_id");

                    foreach (var startTime in startTimes)
                    {
                        var dbStartTime = db.StartTimes.Find(startTime.Id);

                        if (null != dbStartTime)
                        {
                            if (dbStartTime.UserId == idClaim.Value)
                            {
                                db.Remove(dbStartTime);
                            }
                            else
                            {
                                return Results.Problem("No authority to delete");
                            }
                        }
                        else
                        {
                            return Results.Problem("Entry not found");
                        }
                    }

                    await db.SaveChangesAsync();
                    return Results.Ok();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return Results.Problem(ex.Message);
                }
            });

            app.Run();
        }
    }
}
