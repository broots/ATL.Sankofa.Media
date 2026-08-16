using System.Net.Http.Json;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Business.Models.Cloudflare;

namespace ATL.Sankofa.Media.UI.Shared.Services;

public class VideoServiceClient : IVideoService
{
    private readonly HttpClient _httpClient;

    public VideoServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<VideoUploadResponse> InitiateUploadAsync(VideoUploadRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/videos/upload", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VideoUploadResponse>(cancellationToken))!;
    }

    public async Task<VideoDto?> GetVideoAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<VideoDto>($"api/videos/{videoId}", cancellationToken);
    }

    public async Task<VideoListResponse> GetVideosAsync(Guid channelId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return (await _httpClient.GetFromJsonAsync<VideoListResponse>(
            $"api/videos/channel/{channelId}?page={page}&pageSize={pageSize}", cancellationToken))!;
    }

    public async Task<VideoListResponse> GetPublicFeedAsync(int page = 1, int pageSize = 20, string? category = null, CancellationToken cancellationToken = default)
    {
        var url = $"api/videos/feed?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(category)) url += $"&category={Uri.EscapeDataString(category)}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"VideoService API error: {response.StatusCode} for {url}");
            return new VideoListResponse();
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (contentType is not null && !contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"VideoService API returned unexpected content type: {contentType} for {url}");
            return new VideoListResponse();
        }

        return (await response.Content.ReadFromJsonAsync<VideoListResponse>(cancellationToken)) ?? new VideoListResponse();
    }

    public async Task<VideoDto> UpdateVideoAsync(Guid videoId, string? title, string? description, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/videos/{videoId}", new { Title = title, Description = description }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<VideoDto>(cancellationToken))!;
    }

    public async Task PublishVideoAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"api/videos/{videoId}/publish", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteVideoAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/videos/{videoId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<string?> GetPlaybackUrlAsync(Guid videoId, Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<PlaybackUrlResult>($"api/videos/{videoId}/playback-url", cancellationToken);
        return result?.Url;
    }

    public async Task IncrementViewCountAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync($"api/videos/{videoId}/view", null, cancellationToken);
    }

    public async Task SyncVideoStatusAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        await _httpClient.PostAsync($"api/videos/{videoId}/sync-status", null, cancellationToken);
    }

    public Task SyncPendingPublicVideosAsync(int batchSize = 10, CancellationToken cancellationToken = default)
    {
        // Server-only operation; not callable from the UI client.
        throw new NotSupportedException("SyncPendingPublicVideosAsync is a server-side operation.");
    }

    public Task<bool> HandleCloudflareWebhookAsync(CloudflareVideoResult payload, CancellationToken cancellationToken = default)
    {
        // Server-only operation; not callable from the UI client.
        throw new NotSupportedException("HandleCloudflareWebhookAsync is a server-side operation.");
    }

    private class PlaybackUrlResult
    {
        public string? Url { get; set; }
    }
}
