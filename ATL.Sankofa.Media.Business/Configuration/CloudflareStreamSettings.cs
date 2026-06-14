namespace ATL.Sankofa.Media.Business.Configuration;

public class CloudflareStreamSettings
{
    public const string SectionName = "CloudflareStream";

    public string AccountId { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.cloudflare.com/client/v4";
    public string? CustomerSubdomain { get; set; }
    public SignedUrlSettings SignedUrls { get; set; } = new();
}

public class SignedUrlSettings
{
    public bool Enabled { get; set; } = true;
    public string? SigningKeyId { get; set; }
    public string? SigningKey { get; set; }
    public int TokenLifetimeMinutes { get; set; } = 60;
}
