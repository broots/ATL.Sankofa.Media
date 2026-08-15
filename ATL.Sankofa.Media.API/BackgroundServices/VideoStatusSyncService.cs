using ATL.Sankofa.Media.Business.Interfaces;

namespace ATL.Sankofa.Media.API.BackgroundServices;

/// <summary>
/// Periodically syncs the status of public videos that are still in the Processing
/// state. This work used to run inline inside <c>VideoService.GetPublicFeedAsync</c>,
/// which made the public feed endpoint slow and exposed it to database command
/// timeouts (surfacing as <see cref="TaskCanceledException"/>). Moving it to a
/// background service keeps the feed request fast and resilient.
/// </summary>
public class VideoStatusSyncService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    private const int BatchSize = 10;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VideoStatusSyncService> _logger;

    public VideoStatusSyncService(IServiceScopeFactory scopeFactory, ILogger<VideoStatusSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // PeriodicTimer avoids overlapping runs and honors graceful shutdown.
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var videoService = scope.ServiceProvider.GetRequiredService<IVideoService>();
                await videoService.SyncPendingPublicVideosAsync(BatchSize, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown; stop looping.
                break;
            }
            catch (Exception ex)
            {
                // Never let a single failed pass crash the background service.
                _logger.LogError(ex, "Error while syncing pending public video statuses.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
