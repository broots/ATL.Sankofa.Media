namespace ATL.Sankofa.Media.Data.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? VideoId { get; set; }
    public Guid? SubscriptionId { get; set; }

    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? ExternalPaymentId { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Video? Video { get; set; }
}

public enum PaymentType
{
    Subscription,
    PayPerView,
    LiveStreamTicket,
    Tip
}

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded,
    Cancelled
}
