using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Business.Models.Cloudflare;

namespace ATL.Sankofa.Media.Business.Interfaces;

public interface IVideoService
{
    Task<VideoUploadResponse> InitiateUploadAsync(VideoUploadRequest request, CancellationToken cancellationToken = default);
    Task<VideoDto?> GetVideoAsync(Guid videoId, CancellationToken cancellationToken = default);
    Task<VideoListResponse> GetVideosAsync(Guid channelId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<VideoListResponse> GetPublicFeedAsync(int page = 1, int pageSize = 20, string? category = null, CancellationToken cancellationToken = default);
    Task<VideoDto> UpdateVideoAsync(Guid videoId, string? title, string? description, CancellationToken cancellationToken = default);
    Task PublishVideoAsync(Guid videoId, CancellationToken cancellationToken = default);
    Task DeleteVideoAsync(Guid videoId, CancellationToken cancellationToken = default);
    Task<string?> GetPlaybackUrlAsync(Guid videoId, Guid userId, CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(Guid videoId, CancellationToken cancellationToken = default);
    Task SyncVideoStatusAsync(Guid videoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs the status of up to <paramref name="batchSize"/> public videos that are still
    /// in the Processing state. Intended to be called off the request path by a background
    /// service. Per-video failures are swallowed so one bad video does not stop the batch.
    /// </summary>
    Task SyncPendingPublicVideosAsync(int batchSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a Cloudflare Stream webhook notification to the matching video, updating its
    /// status and playback metadata. Returns false if no video matches the Cloudflare uid.
    /// </summary>
    Task<bool> HandleCloudflareWebhookAsync(CloudflareVideoResult payload, CancellationToken cancellationToken = default);
}
