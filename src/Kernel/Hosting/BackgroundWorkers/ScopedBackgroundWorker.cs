using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pylonboard.Kernel.Hosting.BackgroundWorkers;

public class ScopedBackgroundService<TWorker> : BackgroundService where TWorker : IScopedBackgroundServiceWorker
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ScopedBackgroundService<TWorker>> _logger;

    public ScopedBackgroundService(
        IServiceProvider services,
        ILogger<ScopedBackgroundService<TWorker>> logger
    )
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var worker = scope.ServiceProvider.GetRequiredService<TWorker>();
            await worker.DoWorkAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(42, ex, "critical error in hosted service");

            throw;
        }
    }
}