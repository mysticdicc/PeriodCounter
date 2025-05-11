using Microsoft.EntityFrameworkCore;
using PeriodCounterAPI.Data;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using PeriodLib;
using System.Security.Claims;

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
            _logger.LogInformation("Timed Hosted Service running.");

            _timer = new Timer(NotificationChecker, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(60));

            return Task.CompletedTask;
        }

        private async void NotificationChecker(object? state)
        {
            var db = _dbContextFactory.CreateDbContext();
            List<DeviceRegistration> devices = db.DeviceRegistrations.ToList();
            var messaging = FirebaseMessaging.DefaultInstance;

            if (messaging != null) 
            { 
                foreach (var device in devices)
                {
                    var result = db.StartTimes.Where(x => x.UserId == device.UserId).OrderByDescending(x => x.StartTime).First();

                    if (null != result)
                    {
                        var curTime = DateTime.Now;
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

                            await messaging.SendAsync(message);
                        }
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
