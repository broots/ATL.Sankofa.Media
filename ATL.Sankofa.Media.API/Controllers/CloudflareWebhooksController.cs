using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ATL.Sankofa.Media.Business.Configuration;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models.Cloudflare;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ATL.Sankofa.Media.API.Controllers;

/// <summary>
/// Receives Cloudflare Stream webhooks so video status transitions (Processing -> Ready/Failed)
/// are pushed to us instead of being polled inline on the public feed request. Cloudflare signs
/// each request with a <c>Webhook-Signature</c> header of the form
/// <c>time=&lt;unix&gt;,sig1=&lt;hex-hmac-sha256&gt;</c> where the signed payload is
/// <c>"&lt;time&gt;.&lt;rawBody&gt;"</c>.
/// </summary>
[ApiController]
[Route("api/webhooks/cloudflare")]
[AllowAnonymous]
public class CloudflareWebhooksController : ControllerBase
{
    private readonly IVideoService _videoService;
    private readonly CloudflareStreamSettings _settings;
    private readonly ILogger<CloudflareWebhooksController> _logger;

    public CloudflareWebhooksController(
        IVideoService videoService,
        IOptions<CloudflareStreamSettings> settings,
        ILogger<CloudflareWebhooksController> logger)
    {
        _videoService = videoService;
        _settings = settings.Value;
        _logger = logger;
    }

    [HttpPost("stream")]
    public async Task<IActionResult> Stream(CancellationToken cancellationToken)
    {
        // The raw body is required for signature verification, so read it manually.
        string body;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync(cancellationToken);
        }

        var signatureHeader = Request.Headers["Webhook-Signature"].ToString();
        if (!VerifySignature(body, signatureHeader))
        {
            _logger.LogWarning("Rejected Cloudflare Stream webhook with missing or invalid signature.");
            return Unauthorized();
        }

        CloudflareVideoResult? payload;
        try
        {
            payload = JsonSerializer.Deserialize<CloudflareVideoResult>(body);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Cloudflare Stream webhook payload.");
            return BadRequest();
        }

        if (payload == null || string.IsNullOrEmpty(payload.Uid))
        {
            _logger.LogWarning("Cloudflare Stream webhook payload was empty or missing uid.");
            return BadRequest();
        }

        var handled = await _videoService.HandleCloudflareWebhookAsync(payload, cancellationToken);
        if (!handled)
        {
            // No local video matches this uid. Return 200 so Cloudflare does not keep retrying
            // for a video we do not track.
            _logger.LogInformation("No local video found for Cloudflare uid {Uid}; ignoring webhook.", payload.Uid);
        }

        return Ok();
    }

    private bool VerifySignature(string body, string signatureHeader)
    {
        if (string.IsNullOrEmpty(_settings.WebhookSecret))
        {
            _logger.LogWarning("Cloudflare WebhookSecret is not configured; rejecting webhook.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        string? time = null;
        string? sig1 = null;
        foreach (var part in signatureHeader.Split(','))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;

            var keyName = kv[0].Trim();
            if (keyName == "time") time = kv[1].Trim();
            else if (keyName == "sig1") sig1 = kv[1].Trim();
        }

        if (string.IsNullOrEmpty(time) || string.IsNullOrEmpty(sig1))
            return false;

        var signedPayload = $"{time}.{body}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.WebhookSecret));
        var computed = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload)))
            .ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(sig1.ToLowerInvariant()));
    }
}
