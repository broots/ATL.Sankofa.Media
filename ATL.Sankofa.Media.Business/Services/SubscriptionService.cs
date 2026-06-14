using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;
using ATL.Sankofa.Media.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATL.Sankofa.Media.Business.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;

    private static readonly Dictionary<SubscriptionTier, (decimal Monthly, decimal Yearly)> Pricing = new()
    {
        [SubscriptionTier.Free] = (0m, 0m),
        [SubscriptionTier.Basic] = (9.99m, 99.99m),
        [SubscriptionTier.Premium] = (19.99m, 199.99m)
    };

    private static readonly Dictionary<SubscriptionTier, List<string>> TierFeatures = new()
    {
        [SubscriptionTier.Free] = new() { "Access to free content", "Ad-supported viewing", "480p streaming" },
        [SubscriptionTier.Basic] = new() { "Access to Basic content", "Ad-free viewing", "1080p streaming", "Offline downloads" },
        [SubscriptionTier.Premium] = new() { "Access to all content", "Ad-free viewing", "4K streaming", "Offline downloads", "Early access to new releases", "Exclusive live streams" }
    };

    public SubscriptionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        var pricing = Pricing[request.Tier];
        var price = request.BillingCycle == BillingCycle.Monthly ? pricing.Monthly : pricing.Yearly;

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ChannelId = request.ChannelId,
            Tier = request.Tier,
            Status = SubscriptionStatus.Active,
            Price = price,
            BillingCycle = request.BillingCycle,
            StartDate = DateTime.UtcNow,
            NextBillingDate = request.BillingCycle == BillingCycle.Monthly
                ? DateTime.UtcNow.AddMonths(1)
                : DateTime.UtcNow.AddYears(1)
        };

        await _unitOfWork.Repository<Subscription>().AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetSubscriptionAsync(subscription.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create subscription.");
    }

    public async Task<SubscriptionDto?> GetSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Repository<Subscription>().Query()
            .Include(s => s.Channel)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

        if (subscription == null) return null;

        return MapToDto(subscription);
    }

    public async Task<SubscriptionDto?> GetActiveSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Repository<Subscription>().Query()
            .Include(s => s.Channel)
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.ChannelId == null)
            .OrderByDescending(s => s.Tier)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null) return null;

        return MapToDto(subscription);
    }

    public async Task<List<SubscriptionDto>> GetUserSubscriptionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _unitOfWork.Repository<Subscription>().Query()
            .Include(s => s.Channel)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return subscriptions.Select(MapToDto).ToList();
    }

    public async Task CancelSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Repository<Subscription>().GetByIdAsync(subscriptionId, cancellationToken)
            ?? throw new InvalidOperationException("Subscription not found.");

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        subscription.EndDate = subscription.NextBillingDate;
        subscription.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Subscription>().Update(subscription);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SubscriptionTier> GetUserTierAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscription = await _unitOfWork.Repository<Subscription>().Query()
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.ChannelId == null)
            .OrderByDescending(s => s.Tier)
            .FirstOrDefaultAsync(cancellationToken);

        return subscription?.Tier ?? SubscriptionTier.Free;
    }

    public Task<List<SubscriptionPricingDto>> GetPricingAsync()
    {
        var pricing = Pricing.Select(kvp => new SubscriptionPricingDto
        {
            Tier = kvp.Key,
            TierName = kvp.Key.ToString(),
            MonthlyPrice = kvp.Value.Monthly,
            YearlyPrice = kvp.Value.Yearly,
            Features = TierFeatures[kvp.Key]
        }).ToList();

        return Task.FromResult(pricing);
    }

    public async Task<bool> HasActiveSubscriptionAsync(Guid userId, SubscriptionTier minimumTier, CancellationToken cancellationToken = default)
    {
        var userTier = await GetUserTierAsync(userId, cancellationToken);
        return userTier >= minimumTier;
    }

    private static SubscriptionDto MapToDto(Subscription subscription) => new()
    {
        Id = subscription.Id,
        UserId = subscription.UserId,
        ChannelId = subscription.ChannelId,
        ChannelName = subscription.Channel?.Name,
        Tier = subscription.Tier,
        Status = subscription.Status,
        Price = subscription.Price,
        Currency = subscription.Currency,
        BillingCycle = subscription.BillingCycle,
        StartDate = subscription.StartDate,
        EndDate = subscription.EndDate,
        NextBillingDate = subscription.NextBillingDate
    };
}
