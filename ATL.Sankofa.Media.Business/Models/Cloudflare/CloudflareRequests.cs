using System.Text.Json.Serialization;

namespace ATL.Sankofa.Media.Business.Models.Cloudflare;

public class DirectUploadRequest
{
    [JsonPropertyName("maxDurationSeconds")]
    public int MaxDurationSeconds { get; set; } = 3600;

    [JsonPropertyName("requireSignedURLs")]
    public bool RequireSignedUrls { get; set; } = true;

    [JsonPropertyName("allowedOrigins")]
    public List<string>? AllowedOrigins { get; set; }

    [JsonPropertyName("meta")]
    public Dictionary<string, string>? Meta { get; set; }

    [JsonPropertyName("thumbnailTimestampPct")]
    public double? ThumbnailTimestampPct { get; set; }
}

public class DirectUploadResult
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;

    [JsonPropertyName("uploadURL")]
    public string UploadUrl { get; set; } = string.Empty;
}

public class CreateLiveInputRequest
{
    [JsonPropertyName("meta")]
    public Dictionary<string, string>? Meta { get; set; }

    [JsonPropertyName("recording")]
    public CloudflareRecording? Recording { get; set; }

    [JsonPropertyName("defaultCreator")]
    public string? DefaultCreator { get; set; }
}
