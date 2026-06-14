using ATL.Sankofa.Media.Data.Entities;

namespace ATL.Sankofa.Media.Business.Models;

public class VideoUploadRequest
{
    public Guid ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public VideoVisibility Visibility { get; set; } = VideoVisibility.Private;
    public AccessTier RequiredAccessTier { get; set; } = AccessTier.Free;
    public decimal? PurchasePrice { get; set; }
    public int MaxDurationSeconds { get; set; } = 3600;
}

public class VideoUploadResponse
{
    public Guid VideoId { get; set; }
    public string CloudflareVideoId { get; set; } = string.Empty;
    public string UploadUrl { get; set; } = string.Empty;
}

public class VideoDto
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? HlsPlaybackUrl { get; set; }
    public string? DashPlaybackUrl { get; set; }
    public TimeSpan? Duration { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public VideoStatus Status { get; set; }
    public VideoVisibility Visibility { get; set; }
    public AccessTier RequiredAccessTier { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VideoListResponse
{
    public List<VideoDto> Videos { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
