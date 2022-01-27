using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ScyberLog.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope1 = _logger.BeginScope(new { scope = 1 }))
                {
                    _logger.LogTrace("Worker running at: {time}", DateTimeOffset.Now, new { ExtraData = "HelloWorld" });
                    using (var scope2 = _logger.BeginScope(new { scope = 2 }))
                    {
                    _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now, new { ExtraData = "HelloWorld" });
                    }
                }
                _logger.LogWarning("Worker running at: {time}", DateTimeOffset.Now);
                _logger.LogError("Worker running at: {time}", DateTimeOffset.Now);
                try
                { 
                    throw new Exception("Exceptional!"); 
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during execution at {time}", DateTimeOffset.Now);
                }
                _logger.LogCritical("Worker running at: {time}", DateTimeOffset.Now);

                var logMessage = new {
                    TimeStamp = DateTime.Now,
                    Message = "Hello World",
                    Log = "TestLogger",
                    Source = "Application"
                };
                
                _logger.Log(logLevel: LogLevel.Information, eventId: default, state: logMessage, exception: default, formatter: default);

                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}
