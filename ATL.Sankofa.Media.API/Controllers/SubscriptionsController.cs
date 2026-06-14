using System.Security.Claims;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATL.Sankofa.Media.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IPaywallService _paywallService;

    public SubscriptionsController(ISubscriptionService subscriptionService, IPaywallService paywallService)
    {
        _subscriptionService = subscriptionService;
        _paywallService = paywallService;
    }

    [HttpGet("pricing")]
    public async Task<ActionResult<List<SubscriptionPricingDto>>> GetPricing()
    {
        var pricing = await _subscriptionService.GetPricingAsync();
        return Ok(pricing);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<SubscriptionDto>> CreateSubscription(
        [FromBody] CreateSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _subscriptionService.CreateSubscriptionAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{subscriptionId:guid}")]
    public async Task<ActionResult<SubscriptionDto>> GetSubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var sub = await _subscriptionService.GetSubscriptionAsync(subscriptionId, cancellationToken);
        if (sub == null) return NotFound();
        return Ok(sub);
    }

    [Authorize]
    [HttpGet("active")]
    public async Task<ActionResult<SubscriptionDto>> GetActiveSubscription(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sub = await _subscriptionService.GetActiveSubscriptionAsync(userId, cancellationToken);
        if (sub == null) return NotFound();
        return Ok(sub);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<List<SubscriptionDto>>> GetMySubscriptions(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var subs = await _subscriptionService.GetUserSubscriptionsAsync(userId, cancellationToken);
        return Ok(subs);
    }

    [Authorize]
    [HttpPost("{subscriptionId:guid}/cancel")]
    public async Task<IActionResult> CancelSubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        await _subscriptionService.CancelSubscriptionAsync(subscriptionId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("tier")]
    public async Task<ActionResult<SubscriptionTier>> GetMyTier(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var tier = await _subscriptionService.GetUserTierAsync(userId, cancellationToken);
        return Ok(tier);
    }

    [Authorize]
    [HttpPost("purchase-video")]
    public async Task<ActionResult<PaymentDto>> PurchaseVideo(
        [FromBody] PurchaseVideoRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paywallService.PurchaseVideoAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("purchase-livestream")]
    public async Task<ActionResult<PaymentDto>> PurchaseLiveStreamTicket(
        [FromBody] PurchaseLiveStreamTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paywallService.PurchaseLiveStreamTicketAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("access/video/{videoId:guid}")]
    public async Task<ActionResult<AccessCheckResult>> CheckVideoAccess(Guid videoId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _paywallService.CheckVideoAccessAsync(userId, videoId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("access/livestream/{liveStreamId:guid}")]
    public async Task<ActionResult<AccessCheckResult>> CheckLiveStreamAccess(Guid liveStreamId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _paywallService.CheckLiveStreamAccessAsync(userId, liveStreamId, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("payments")]
    public async Task<ActionResult<List<PaymentDto>>> GetMyPayments(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var payments = await _paywallService.GetUserPaymentsAsync(userId, cancellationToken);
        return Ok(payments);
    }
}
