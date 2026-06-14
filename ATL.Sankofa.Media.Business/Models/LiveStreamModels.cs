using ATL.Sankofa.Media.Data.Entities;

namespace ATL.Sankofa.Media.Business.Models;

public class CreateLiveStreamRequest
{
    public Guid ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AccessTier RequiredAccessTier { get; set; } = AccessTier.Free;
    public decimal? TicketPrice { get; set; }
    public DateTime? ScheduledStartTime { get; set; }
    public bool RecordStream { get; set; } = true;
}

public class LiveStreamDto
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? HlsPlaybackUrl { get; set; }
    public string? RtmpsUrl { get; set; }
    public string? RtmpsStreamKey { get; set; }
    public string? SrtUrl { get; set; }
    public string? WebRtcUrl { get; set; }
    public LiveStreamStatus Status { get; set; }
    public AccessTier RequiredAccessTier { get; set; }
    public decimal? TicketPrice { get; set; }
    public long ConcurrentViewers { get; set; }
    public DateTime? ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LiveStreamListResponse
{
    public List<LiveStreamDto> LiveStreams { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
