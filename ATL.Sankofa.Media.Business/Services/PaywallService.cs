using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;
using ATL.Sankofa.Media.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATL.Sankofa.Media.Business.Services;

public class PaywallService : IPaywallService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISubscriptionService _subscriptionService;

    public PaywallService(IUnitOfWork unitOfWork, ISubscriptionService subscriptionService)
    {
        _unitOfWork = unitOfWork;
        _subscriptionService = subscriptionService;
    }

    public async Task<AccessCheckResult> CheckVideoAccessAsync(Guid userId, Guid videoId, CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Repository<Video>().GetByIdAsync(videoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        if (video.RequiredAccessTier == AccessTier.Free)
        {
            return new AccessCheckResult { HasAccess = true };
        }

        if (video.RequiredAccessTier == AccessTier.PayPerView)
        {
            var hasPurchased = await HasPurchasedVideoAsync(userId, videoId, cancellationToken);
            return new AccessCheckResult
            {
                HasAccess = hasPurchased,
                DenialReason = hasPurchased ? null : "This video requires a one-time purchase.",
                RequiredTier = video.RequiredAccessTier,
                PurchasePrice = video.PurchasePrice
            };
        }

        var requiredSubscriptionTier = video.RequiredAccessTier switch
        {
            AccessTier.Basic => SubscriptionTier.Basic,
            AccessTier.Premium => SubscriptionTier.Premium,
            _ => SubscriptionTier.Free
        };

        var hasSubscription = await _subscriptionService.HasActiveSubscriptionAsync(userId, requiredSubscriptionTier, cancellationToken);
        return new AccessCheckResult
        {
            HasAccess = hasSubscription,
            DenialReason = hasSubscription ? null : $"This video requires a {requiredSubscriptionTier} subscription or higher.",
            RequiredTier = video.RequiredAccessTier
        };
    }

    public async Task<AccessCheckResult> CheckLiveStreamAccessAsync(Guid userId, Guid liveStreamId, CancellationToken cancellationToken = default)
    {
        var liveStream = await _unitOfWork.Repository<Data.Entities.LiveStream>().GetByIdAsync(liveStreamId, cancellationToken)
            ?? throw new InvalidOperationException("Live stream not found.");

        if (liveStream.RequiredAccessTier == AccessTier.Free)
        {
            return new AccessCheckResult { HasAccess = true };
        }

        if (liveStream.TicketPrice.HasValue && liveStream.TicketPrice > 0)
        {
            var hasTicket = await _unitOfWork.Repository<Payment>().AnyAsync(
                p => p.UserId == userId &&
                     p.Type == PaymentType.LiveStreamTicket &&
                     p.Status == PaymentStatus.Completed &&
                     p.Description != null && p.Description.Contains(liveStreamId.ToString()),
                cancellationToken);

            return new AccessCheckResult
            {
                HasAccess = hasTicket,
                DenialReason = hasTicket ? null : "This live stream requires a ticket purchase.",
                RequiredTier = liveStream.RequiredAccessTier,
                PurchasePrice = liveStream.TicketPrice
            };
        }

        var requiredSubscriptionTier = liveStream.RequiredAccessTier switch
        {
            AccessTier.Basic => SubscriptionTier.Basic,
            AccessTier.Premium => SubscriptionTier.Premium,
            _ => SubscriptionTier.Free
        };

        var hasSubscription = await _subscriptionService.HasActiveSubscriptionAsync(userId, requiredSubscriptionTier, cancellationToken);
        return new AccessCheckResult
        {
            HasAccess = hasSubscription,
            DenialReason = hasSubscription ? null : $"This live stream requires a {requiredSubscriptionTier} subscription or higher.",
            RequiredTier = liveStream.RequiredAccessTier
        };
    }

    public async Task<PaymentDto> PurchaseVideoAsync(PurchaseVideoRequest request, CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Repository<Video>().GetByIdAsync(request.VideoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        if (!video.PurchasePrice.HasValue || video.PurchasePrice <= 0)
        {
            throw new InvalidOperationException("This video is not available for purchase.");
        }

        var existingPayment = await _unitOfWork.Repository<Payment>().FirstOrDefaultAsync(
            p => p.UserId == request.UserId && p.VideoId == request.VideoId && p.Status == PaymentStatus.Completed,
            cancellationToken);

        if (existingPayment != null)
        {
            throw new InvalidOperationException("You have already purchased this video.");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            VideoId = request.VideoId,
            Type = PaymentType.PayPerView,
            Amount = video.PurchasePrice.Value,
            Status = PaymentStatus.Completed,
            PaymentMethod = request.PaymentMethod,
            Description = $"Purchase of video: {video.Title}",
            CompletedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(payment, video.Title);
    }

    public async Task<PaymentDto> PurchaseLiveStreamTicketAsync(PurchaseLiveStreamTicketRequest request, CancellationToken cancellationToken = default)
    {
        var liveStream = await _unitOfWork.Repository<Data.Entities.LiveStream>().GetByIdAsync(request.LiveStreamId, cancellationToken)
            ?? throw new InvalidOperationException("Live stream not found.");

        if (!liveStream.TicketPrice.HasValue || liveStream.TicketPrice <= 0)
        {
            throw new InvalidOperationException("This live stream does not require a ticket.");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = PaymentType.LiveStreamTicket,
            Amount = liveStream.TicketPrice.Value,
            Status = PaymentStatus.Completed,
            PaymentMethod = request.PaymentMethod,
            Description = $"Ticket for live stream: {liveStream.Title} [{liveStream.Id}]",
            CompletedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(payment, liveStream.Title);
    }

    public async Task<List<PaymentDto>> GetUserPaymentsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var payments = await _unitOfWork.Repository<Payment>().Query()
            .Include(p => p.Video)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return payments.Select(p => MapToDto(p, p.Video?.Title)).ToList();
    }

    public async Task<bool> HasPurchasedVideoAsync(Guid userId, Guid videoId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Repository<Payment>().AnyAsync(
            p => p.UserId == userId && p.VideoId == videoId && p.Status == PaymentStatus.Completed,
            cancellationToken);
    }

    private static PaymentDto MapToDto(Payment payment, string? videoTitle = null) => new()
    {
        Id = payment.Id,
        UserId = payment.UserId,
        VideoId = payment.VideoId,
        VideoTitle = videoTitle,
        Type = payment.Type,
        Amount = payment.Amount,
        Currency = payment.Currency,
        Status = payment.Status,
        Description = payment.Description,
        CreatedAt = payment.CreatedAt,
        CompletedAt = payment.CompletedAt
    };
}
