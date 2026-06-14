using System.Text.Json.Serialization;

namespace ATL.Sankofa.Media.Business.Models.Cloudflare;

public class CloudflareLiveInputResult
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = string.Empty;

    [JsonPropertyName("rtmps")]
    public CloudflareRtmps? Rtmps { get; set; }

    [JsonPropertyName("rtmpsPlayback")]
    public CloudflareRtmps? RtmpsPlayback { get; set; }

    [JsonPropertyName("srt")]
    public CloudflareSrt? Srt { get; set; }

    [JsonPropertyName("srtPlayback")]
    public CloudflareSrt? SrtPlayback { get; set; }

    [JsonPropertyName("webRTC")]
    public CloudflareWebRtc? WebRtc { get; set; }

    [JsonPropertyName("webRTCPlayback")]
    public CloudflareWebRtc? WebRtcPlayback { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("created")]
    public DateTime? Created { get; set; }

    [JsonPropertyName("modified")]
    public DateTime? Modified { get; set; }

    [JsonPropertyName("recording")]
    public CloudflareRecording? Recording { get; set; }
}

public class CloudflareRtmps
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("streamKey")]
    public string StreamKey { get; set; } = string.Empty;
}

public class CloudflareSrt
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("streamId")]
    public string StreamId { get; set; } = string.Empty;

    [JsonPropertyName("passphrase")]
    public string Passphrase { get; set; } = string.Empty;
}

public class CloudflareWebRtc
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public class CloudflareRecording
{
    [JsonPropertyName("mode")]
    public string Mode { get; set; } = "automatic";

    [JsonPropertyName("requireSignedURLs")]
    public bool RequireSignedUrls { get; set; }

    [JsonPropertyName("allowedOrigins")]
    public List<string>? AllowedOrigins { get; set; }
}
