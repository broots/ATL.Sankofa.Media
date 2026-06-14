using System.Net.Http.Json;
using System.Text.Json;
using ATL.Sankofa.Media.Business.Configuration;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models.Cloudflare;
using Microsoft.Extensions.Options;

namespace ATL.Sankofa.Media.Business.Services;

public class CloudflareStreamClient : ICloudflareStreamClient
{
    private readonly HttpClient _httpClient;
    private readonly CloudflareStreamSettings _settings;

    public CloudflareStreamClient(HttpClient httpClient, IOptions<CloudflareStreamSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;

        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiToken}");
    }

    public async Task<DirectUploadResult> CreateDirectUploadAsync(DirectUploadRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"/client/v4/accounts/{_settings.AccountId}/stream/direct_upload";
        var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CloudflareResponse<DirectUploadResult>>(cancellationToken: cancellationToken);
        if (result?.Success != true || result.Result == null)
        {
            throw new InvalidOperationException($"Cloudflare API error: {string.Join(", ", result?.Errors.Select(e => e.Message) ?? Array.Empty<string>())}");
        }

        return result.Result;
    }

    public async Task<CloudflareVideoResult?> GetVideoAsync(string videoId, CancellationToken cancellationToken = default)
    {
        var url = $"/client/v4/accounts/{_settings.AccountId}/stream/{videoId}";
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<CloudflareResponse<CloudflareVideoResult>>(cancellationToken: cancellationToken);
        return result?.Result;
    }

    public async Task DeleteVideoAsync(string videoId, CancellationToken cancellationToken = default)
    {
        var url = $"/client/v4/accounts/{_settings.AccountId}/stream/{videoId}";
        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<CloudflareLiveInputResult> CreateLiveInputAsync(CreateLiveInputRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"/client/v4/accounts/{_settings.AccountId}/stream/live_inputs";
        var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CloudflareResponse<CloudflareLiveInputResult>>(cancellationToken: cancellationToken);
        if (result?.Success != true || result.Result == null)
        {
            throw new InvalidOperationException($"Cloudflare API error: {string.Join(", ", result?.Errors.Select(e => e.Message) ?? Array.Empty<string>())}");
        }

        return result.Result;
    }

    public async Task<CloudflareLiveInputResult?> GetLiveInputAsync(string liveInputId, CancellationToken cancellationToken = default)
    {
        var url = $"/client/v4/accounts/{_settings.AccountId}/stream/live_inputs/{liveInputId}";
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<CloudflareResponse<CloudflareLiveInputResult>>(cancellationToken: cancellationToken);
        return result?.Result;
    }

    public async Task DeleteLiveInputAsync(string liveInputId, CancellationToken cancellationToken = default)
    {
        var url = $"/client/v4/accounts/{_settings.AccountId}/stream/live_inputs/{liveInputId}";
        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public Task<string> GenerateSignedTokenAsync(string videoId, int lifetimeMinutes = 60)
    {
        // Signed URL token generation using the signing key
        // In production, this would use JWT with the Cloudflare signing key
        if (string.IsNullOrEmpty(_settings.SignedUrls.SigningKeyId) || string.IsNullOrEmpty(_settings.SignedUrls.SigningKey))
        {
            throw new InvalidOperationException("Signed URL configuration is not set up.");
        }

        var token = GenerateCloudflareToken(videoId, lifetimeMinutes);
        return Task.FromResult(token);
    }

    private string GenerateCloudflareToken(string videoId, int lifetimeMinutes)
    {
        // Base64url-encoded header and payload for Cloudflare Stream signed URLs
        var header = JsonSerializer.Serialize(new { alg = "RS256", kid = _settings.SignedUrls.SigningKeyId });
        var payload = JsonSerializer.Serialize(new
        {
            sub = videoId,
            kid = _settings.SignedUrls.SigningKeyId,
            exp = DateTimeOffset.UtcNow.AddMinutes(lifetimeMinutes).ToUnixTimeSeconds()
        });

        var headerBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(header))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var payloadBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        // In production, sign with RSA private key
        return $"{headerBase64}.{payloadBase64}.signature_placeholder";
    }
}
