namespace Pylonboard.Infrastructure.Hosting.BackgroundWorkers;

public interface IScopedBackgroundServiceWorker
{
    Task DoWorkAsync(CancellationToken stoppingToken);
}