namespace ATL.Sankofa.Media.Data.Entities;

public class LiveStream
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Cloudflare Stream Live Input fields
    public string CloudflareLiveInputId { get; set; } = string.Empty;
    public string? RtmpsUrl { get; set; }
    public string? RtmpsStreamKey { get; set; }
    public string? SrtUrl { get; set; }
    public string? WebRtcUrl { get; set; }
    public string? HlsPlaybackUrl { get; set; }

    public string? ThumbnailUrl { get; set; }
    public LiveStreamStatus Status { get; set; } = LiveStreamStatus.Idle;
    public AccessTier RequiredAccessTier { get; set; } = AccessTier.Free;
    public decimal? TicketPrice { get; set; }

    public long ConcurrentViewers { get; set; }
    public long TotalViewers { get; set; }

    public DateTime? ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Recording options
    public bool RecordStream { get; set; } = true;
    public Guid? RecordedVideoId { get; set; }

    // Navigation properties
    public Channel Channel { get; set; } = null!;
}

public enum LiveStreamStatus
{
    Idle,
    Scheduled,
    Live,
    Ended,
    Failed
}
