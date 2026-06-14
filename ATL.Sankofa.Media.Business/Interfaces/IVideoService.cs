using ATL.Sankofa.Media.Business.Models;

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
}
