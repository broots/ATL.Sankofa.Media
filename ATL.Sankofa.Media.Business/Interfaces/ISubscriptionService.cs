using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;

namespace ATL.Sankofa.Media.Business.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default);
    Task<SubscriptionDto?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<SubscriptionDto?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<SubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CancelSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<SubscriptionTier> GetUserTierAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<SubscriptionPricingDto>> GetPricingAsync();
    Task<bool> HasActiveSubscriptionAsync(Guid userId, SubscriptionTier minimumTier, CancellationToken cancellationToken = default);
}
