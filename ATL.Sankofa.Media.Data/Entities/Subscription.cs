namespace ATL.Sankofa.Media.Data.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ChannelId { get; set; }
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public DateTime? CancelledAt { get; set; }

    public string? ExternalSubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Channel? Channel { get; set; }
}

public enum SubscriptionTier
{
    Free,
    Basic,
    Premium
}

public enum SubscriptionStatus
{
    Active,
    Cancelled,
    Expired,
    PastDue,
    Paused
}

public enum BillingCycle
{
    Monthly,
    Yearly
}
