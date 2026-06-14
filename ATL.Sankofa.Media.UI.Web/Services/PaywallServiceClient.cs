using System.Net.Http.Json;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;

namespace ATL.Sankofa.Media.UI.Web.Services;

public class PaywallServiceClient : IPaywallService
{
    private readonly HttpClient _httpClient;

    public PaywallServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AccessCheckResult> CheckVideoAccessAsync(Guid userId, Guid videoId, CancellationToken cancellationToken = default)
    {
        return (await _httpClient.GetFromJsonAsync<AccessCheckResult>($"api/subscriptions/access/video/{videoId}", cancellationToken))!;
    }

    public async Task<AccessCheckResult> CheckLiveStreamAccessAsync(Guid userId, Guid liveStreamId, CancellationToken cancellationToken = default)
    {
        return (await _httpClient.GetFromJsonAsync<AccessCheckResult>($"api/subscriptions/access/livestream/{liveStreamId}", cancellationToken))!;
    }

    public async Task<PaymentDto> PurchaseVideoAsync(PurchaseVideoRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/subscriptions/purchase-video", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PaymentDto>(cancellationToken))!;
    }

    public async Task<PaymentDto> PurchaseLiveStreamTicketAsync(PurchaseLiveStreamTicketRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/subscriptions/purchase-livestream", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PaymentDto>(cancellationToken))!;
    }

    public async Task<List<PaymentDto>> GetUserPaymentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return (await _httpClient.GetFromJsonAsync<List<PaymentDto>>("api/subscriptions/payments", cancellationToken))!;
    }

    public async Task<bool> HasPurchasedVideoAsync(Guid userId, Guid videoId, CancellationToken cancellationToken = default)
    {
        var access = await CheckVideoAccessAsync(userId, videoId, cancellationToken);
        return access.HasAccess;
    }
}
