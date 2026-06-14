using System.Text.Json.Serialization;

namespace ATL.Sankofa.Media.Business.Models.Cloudflare;

public class CloudflareVideoResult
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;

    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }

    [JsonPropertyName("readyToStream")]
    public bool ReadyToStream { get; set; }

    [JsonPropertyName("status")]
    public CloudflareVideoStatus? Status { get; set; }

    [JsonPropertyName("meta")]
    public Dictionary<string, string>? Meta { get; set; }

    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    [JsonPropertyName("modified")]
    public DateTime? Modified { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("preview")]
    public string? Preview { get; set; }

    [JsonPropertyName("playback")]
    public CloudflarePlayback? Playback { get; set; }

    [JsonPropertyName("input")]
    public CloudflareVideoInput? Input { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("uploadExpiry")]
    public DateTime? UploadExpiry { get; set; }
}

public class CloudflareVideoStatus
{
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("pctComplete")]
    public string? PctComplete { get; set; }

    [JsonPropertyName("errorReasonCode")]
    public string? ErrorReasonCode { get; set; }

    [JsonPropertyName("errorReasonText")]
    public string? ErrorReasonText { get; set; }
}

public class CloudflarePlayback
{
    [JsonPropertyName("hls")]
    public string? Hls { get; set; }

    [JsonPropertyName("dash")]
    public string? Dash { get; set; }
}

public class CloudflareVideoInput
{
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }
}
