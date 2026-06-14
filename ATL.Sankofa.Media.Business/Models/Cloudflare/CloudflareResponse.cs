using System.Text.Json.Serialization;

namespace ATL.Sankofa.Media.Business.Models.Cloudflare;

// Base response wrapper from Cloudflare API
public class CloudflareResponse<T>
{
    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errors")]
    public List<CloudflareError> Errors { get; set; } = new();

    [JsonPropertyName("messages")]
    public List<CloudflareMessage> Messages { get; set; } = new();
}

public class CloudflareError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class CloudflareMessage
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
