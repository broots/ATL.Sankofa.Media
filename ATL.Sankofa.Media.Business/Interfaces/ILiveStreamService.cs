using ATL.Sankofa.Media.Business.Models;

namespace ATL.Sankofa.Media.Business.Interfaces;

public interface ILiveStreamService
{
    Task<LiveStreamDto> CreateLiveStreamAsync(CreateLiveStreamRequest request, CancellationToken cancellationToken = default);
    Task<LiveStreamDto?> GetLiveStreamAsync(Guid liveStreamId, CancellationToken cancellationToken = default);
    Task<LiveStreamListResponse> GetLiveStreamsAsync(Guid channelId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<LiveStreamListResponse> GetActiveLiveStreamsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<string?> GetPlaybackUrlAsync(Guid liveStreamId, Guid userId, CancellationToken cancellationToken = default);
    Task EndLiveStreamAsync(Guid liveStreamId, CancellationToken cancellationToken = default);
    Task DeleteLiveStreamAsync(Guid liveStreamId, CancellationToken cancellationToken = default);
    Task UpdateViewerCountAsync(Guid liveStreamId, long concurrentViewers, CancellationToken cancellationToken = default);
}
