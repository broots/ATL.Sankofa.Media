using System.Net.Http.Json;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;

namespace ATL.Sankofa.Media.UI.Shared.Services;

public class ChannelServiceClient : IChannelService
{
    private readonly HttpClient _httpClient;

    public ChannelServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ChannelDto> CreateChannelAsync(CreateChannelRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/channels", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ChannelDto>(cancellationToken))!;
    }

    public async Task<ChannelDto?> GetChannelAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<ChannelDto>($"api/channels/{channelId}", cancellationToken);
    }

    public async Task<ChannelDto?> GetChannelByHandleAsync(string handle, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<ChannelDto>($"api/channels/handle/{Uri.EscapeDataString(handle)}", cancellationToken);
    }

    public async Task<ChannelDto> UpdateChannelAsync(Guid channelId, UpdateChannelRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/channels/{channelId}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ChannelDto>(cancellationToken))!;
    }

    public async Task<bool> IsHandleAvailableAsync(string handle, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<bool>($"api/channels/handle/{Uri.EscapeDataString(handle)}/available", cancellationToken);
    }

    public async Task IncrementSubscriberCountAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        // Managed server-side, not called from client
    }

    public async Task DecrementSubscriberCountAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        // Managed server-side, not called from client
    }
}
