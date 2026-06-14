using System.Net.Http.Json;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;

namespace ATL.Sankofa.Media.UI.Web.Services;

public class SubscriptionServiceClient : ISubscriptionService
{
    private readonly HttpClient _httpClient;

    public SubscriptionServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/subscriptions", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SubscriptionDto>(cancellationToken))!;
    }

    public async Task<SubscriptionDto?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<SubscriptionDto>($"api/subscriptions/{subscriptionId}", cancellationToken);
    }

    public async Task<SubscriptionDto?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SubscriptionDto>("api/subscriptions/active", cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<SubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return (await _httpClient.GetFromJsonAsync<List<SubscriptionDto>>("api/subscriptions/my", cancellationToken))!;
    }

    public async Task CancelSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"api/subscriptions/{subscriptionId}/cancel", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<SubscriptionTier> GetUserTierAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<SubscriptionTier>("api/subscriptions/tier", cancellationToken);
    }

    public async Task<List<SubscriptionPricingDto>> GetPricingAsync()
    {
        return (await _httpClient.GetFromJsonAsync<List<SubscriptionPricingDto>>("api/subscriptions/pricing"))!;
    }

    public async Task<bool> HasActiveSubscriptionAsync(Guid userId, SubscriptionTier minimumTier, CancellationToken cancellationToken = default)
    {
        var tier = await GetUserTierAsync(userId, cancellationToken);
        return tier >= minimumTier;
    }
}
