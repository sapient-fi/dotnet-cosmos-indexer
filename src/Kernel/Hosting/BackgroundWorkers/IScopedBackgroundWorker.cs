namespace Pylonboard.Kernel.Hosting.BackgroundWorkers;

public interface IScopedBackgroundServiceWorker
{
    Task DoWorkAsync(CancellationToken stoppingToken);
}