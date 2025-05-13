using Microsoft.EntityFrameworkCore;
using PeriodCounterAPI.Data;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using PeriodLib;
using System.Security.Claims;
using Microsoft.Extensions.Logging.EventLog;

namespace PeriodCounterAPI
{
    public class NotificationService : IHostedService, IDisposable
    {
        private readonly ILogger<NotificationService> _logger;
        private Timer? _timer = null;
        private IDbContextFactory<PeriodDb> _dbContextFactory;

        public NotificationService(ILogger<NotificationService> logger, IDbContextFactory<PeriodDb> dbContextFactory)
        {
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogWarning("Timed Hosted Service running.");

            _timer = new Timer(NotificationChecker, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        private async void NotificationChecker(object? state)
        {
            _logger.LogWarning("Notification service started");

            string resultsString = String.Empty;

            try
            {
                var db = _dbContextFactory.CreateDbContext();
                List<DeviceRegistration> devices = db.DeviceRegistrations.ToList();
                var messaging = FirebaseMessaging.DefaultInstance;

                if (messaging != null)
                {
                    foreach (var device in devices)
                    {
                        resultsString = resultsString + $"\nChecking {device.Fcm}";

                        var curTime = DateTime.Now;
                        var notificationDiff = curTime - device.LastNotificationSent;
                        var notificationTimespan = TimeSpan.FromHours(24);

                        if (notificationDiff > notificationTimespan)
                        {
                            var resultList = db.StartTimes.Where(x => x.UserId == device.UserId).OrderByDescending(x => x.StartTime).ToList();

                            if (resultList.Count > 0)
                            {
                                var result = resultList.First();

                                if (null != result)
                                {
                                    var timespan = TimeSpan.FromDays(25);
                                    var diff = curTime - result.StartTime;

                                    if (diff > timespan)
                                    {
                                        var message = new Message()
                                        {
                                            Notification = new Notification()
                                            {
                                                Title = "Period Alert",
                                                Body = $"It has been {diff.Days} days since your last period"
                                            },
                                            Token = device.Fcm
                                        };

                                        try
                                        {
                                            await messaging.SendAsync(message);
                                            device.LastNotificationSent = curTime;
                                            resultsString = resultsString + $"\nMessage sent to {device.Fcm}";
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex.ToString());
                                        }
                                    }
                                    else
                                    {
                                        resultsString = resultsString + $"\nMessage not sent to {device.Fcm} as hasn't been longer than 25 days";
                                    }
                                }
                            }
                            else
                            {
                                resultsString = resultsString + $"\nCheck skipped for {device.Fcm} due to lack of results";
                            }
                        }
                        else
                        {
                            resultsString = resultsString + $"\nCheck skipped for {device.Fcm} due to notification being sent to recently";
                        }
                    }
                    await db.SaveChangesAsync();
                }
            } 
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            _logger.LogWarning("Notification service finished" + resultsString);
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogWarning("Timed Hosted Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
