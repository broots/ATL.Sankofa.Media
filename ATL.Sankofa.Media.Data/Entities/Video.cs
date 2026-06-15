namespace ATL.Sankofa.Media.Data.Entities;

public class Video
{
    public Guid Id { get; set; }
    public Guid? ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Cloudflare Stream fields
    public string CloudflareVideoId { get; set; } = string.Empty;
    public string? CloudflareVideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? HlsPlaybackUrl { get; set; }
    public string? DashPlaybackUrl { get; set; }

    public TimeSpan? Duration { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long DislikeCount { get; set; }

    public VideoStatus Status { get; set; } = VideoStatus.Processing;
    public VideoVisibility Visibility { get; set; } = VideoVisibility.Private;
    public AccessTier RequiredAccessTier { get; set; } = AccessTier.Free;
    public decimal? PurchasePrice { get; set; }

    public string? Tags { get; set; }
    public string? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Channel? Channel { get; set; }
    public ICollection<WatchHistory> WatchHistories { get; set; } = new List<WatchHistory>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public enum VideoStatus
{
    Processing,
    Ready,
    Failed,
    Deleted
}

public enum VideoVisibility
{
    Private,
    Unlisted,
    Public
}

public enum AccessTier
{
    Free,
    Basic,
    Premium,
    PayPerView
}
