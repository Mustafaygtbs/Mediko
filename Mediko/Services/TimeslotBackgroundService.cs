using Mediko.Services.Interfaces;

namespace Mediko.Services
{
    public class TimeslotBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TimeslotBackgroundService> _logger;

        public TimeslotBackgroundService(IServiceProvider serviceProvider, ILogger<TimeslotBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timeslot background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var timeslotService = scope.ServiceProvider.GetRequiredService<ITimeslotService>();

                        var now = DateTime.Now;
                        var nextRun = DateTime.Today.AddDays(1).AddHours(2); // Ertesi gün 02:00
                        var delay = nextRun - now;
                        if (delay < TimeSpan.Zero)
                        {
                            delay = TimeSpan.Zero;
                        }
                        _logger.LogInformation($"Timeslot service sleeping for {delay} until next run.");
                        await Task.Delay(delay, stoppingToken);

                        // Yeni slotları oluştur 
                        await timeslotService.GenerateTimeslotsForNextDaysAsync(2);
                        _logger.LogInformation("Timeslots generated for next days.");

                        // Eski slotları temizle
                        await timeslotService.RemoveOldTimeslotsAsync(7);
                        _logger.LogInformation("Old timeslots removed.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the TimeslotBackgroundService.");
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }

            _logger.LogInformation("Timeslot background service stopping.");
        }
    }
}
