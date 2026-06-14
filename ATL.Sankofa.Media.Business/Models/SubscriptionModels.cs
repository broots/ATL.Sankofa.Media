using ATL.Sankofa.Media.Data.Entities;

namespace ATL.Sankofa.Media.Business.Models;

public class CreateSubscriptionRequest
{
    public Guid UserId { get; set; }
    public Guid? ChannelId { get; set; }
    public SubscriptionTier Tier { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
}

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ChannelId { get; set; }
    public string? ChannelName { get; set; }
    public SubscriptionTier Tier { get; set; }
    public SubscriptionStatus Status { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public BillingCycle BillingCycle { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
}

public class SubscriptionPricingDto
{
    public SubscriptionTier Tier { get; set; }
    public string TierName { get; set; } = string.Empty;
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public List<string> Features { get; set; } = new();
}
