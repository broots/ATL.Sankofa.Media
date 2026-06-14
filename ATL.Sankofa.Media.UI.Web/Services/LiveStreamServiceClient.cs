using System.Net.Http.Json;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;

namespace ATL.Sankofa.Media.UI.Web.Services;

public class LiveStreamServiceClient : ILiveStreamService
{
    private readonly HttpClient _httpClient;

    public LiveStreamServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LiveStreamDto> CreateLiveStreamAsync(CreateLiveStreamRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/livestreams", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LiveStreamDto>(cancellationToken))!;
    }

    public async Task<LiveStreamDto?> GetLiveStreamAsync(Guid liveStreamId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<LiveStreamDto>($"api/livestreams/{liveStreamId}", cancellationToken);
    }

    public async Task<LiveStreamListResponse> GetLiveStreamsAsync(Guid channelId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return (await _httpClient.GetFromJsonAsync<LiveStreamListResponse>(
            $"api/livestreams/channel/{channelId}?page={page}&pageSize={pageSize}", cancellationToken))!;
    }

    public async Task<LiveStreamListResponse> GetActiveLiveStreamsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return (await _httpClient.GetFromJsonAsync<LiveStreamListResponse>(
            $"api/livestreams/active?page={page}&pageSize={pageSize}", cancellationToken))!;
    }

    public async Task<string?> GetPlaybackUrlAsync(Guid liveStreamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<PlaybackUrlResult>($"api/livestreams/{liveStreamId}/playback-url", cancellationToken);
        return result?.Url;
    }

    public async Task EndLiveStreamAsync(Guid liveStreamId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"api/livestreams/{liveStreamId}/end", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteLiveStreamAsync(Guid liveStreamId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/livestreams/{liveStreamId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateViewerCountAsync(Guid liveStreamId, long concurrentViewers, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/livestreams/{liveStreamId}/viewers", new { ConcurrentViewers = concurrentViewers }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private class PlaybackUrlResult
    {
        public string? Url { get; set; }
    }
}
