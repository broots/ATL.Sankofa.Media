using ATL.Sankofa.Media.Data.Entities;

namespace ATL.Sankofa.Media.Business.Models;

public class PurchaseVideoRequest
{
    public Guid UserId { get; set; }
    public Guid VideoId { get; set; }
    public string? PaymentMethod { get; set; }
}

public class PurchaseLiveStreamTicketRequest
{
    public Guid UserId { get; set; }
    public Guid LiveStreamId { get; set; }
    public string? PaymentMethod { get; set; }
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? VideoId { get; set; }
    public string? VideoTitle { get; set; }
    public PaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class AccessCheckResult
{
    public bool HasAccess { get; set; }
    public string? DenialReason { get; set; }
    public AccessTier RequiredTier { get; set; }
    public decimal? PurchasePrice { get; set; }
}
