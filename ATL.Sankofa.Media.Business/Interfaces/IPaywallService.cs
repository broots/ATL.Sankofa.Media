using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;

namespace ATL.Sankofa.Media.Business.Interfaces;

public interface IPaywallService
{
    Task<AccessCheckResult> CheckVideoAccessAsync(Guid userId, Guid videoId, CancellationToken cancellationToken = default);
    Task<AccessCheckResult> CheckLiveStreamAccessAsync(Guid userId, Guid liveStreamId, CancellationToken cancellationToken = default);
    Task<PaymentDto> PurchaseVideoAsync(PurchaseVideoRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDto> PurchaseLiveStreamTicketAsync(PurchaseLiveStreamTicketRequest request, CancellationToken cancellationToken = default);
    Task<List<PaymentDto>> GetUserPaymentsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasPurchasedVideoAsync(Guid userId, Guid videoId, CancellationToken cancellationToken = default);
}
