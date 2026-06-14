using System.Security.Claims;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATL.Sankofa.Media.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LiveStreamsController : ControllerBase
{
    private readonly ILiveStreamService _liveStreamService;

    public LiveStreamsController(ILiveStreamService liveStreamService)
    {
        _liveStreamService = liveStreamService;
    }

    [HttpGet("active")]
    public async Task<ActionResult<LiveStreamListResponse>> GetActiveLiveStreams(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _liveStreamService.GetActiveLiveStreamsAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{liveStreamId:guid}")]
    public async Task<ActionResult<LiveStreamDto>> GetLiveStream(Guid liveStreamId, CancellationToken cancellationToken)
    {
        var stream = await _liveStreamService.GetLiveStreamAsync(liveStreamId, cancellationToken);
        if (stream == null) return NotFound();
        return Ok(stream);
    }

    [HttpGet("channel/{channelId:guid}")]
    public async Task<ActionResult<LiveStreamListResponse>> GetChannelLiveStreams(
        Guid channelId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _liveStreamService.GetLiveStreamsAsync(channelId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<LiveStreamDto>> CreateLiveStream(
        [FromBody] CreateLiveStreamRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _liveStreamService.CreateLiveStreamAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("{liveStreamId:guid}/playback-url")]
    public async Task<ActionResult<PlaybackUrlResponse>> GetPlaybackUrl(Guid liveStreamId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var url = await _liveStreamService.GetPlaybackUrlAsync(liveStreamId, userId, cancellationToken);
        return Ok(new PlaybackUrlResponse { Url = url });
    }

    [Authorize]
    [HttpPost("{liveStreamId:guid}/end")]
    public async Task<IActionResult> EndLiveStream(Guid liveStreamId, CancellationToken cancellationToken)
    {
        await _liveStreamService.EndLiveStreamAsync(liveStreamId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{liveStreamId:guid}")]
    public async Task<IActionResult> DeleteLiveStream(Guid liveStreamId, CancellationToken cancellationToken)
    {
        await _liveStreamService.DeleteLiveStreamAsync(liveStreamId, cancellationToken);
        return NoContent();
    }

    [HttpPut("{liveStreamId:guid}/viewers")]
    public async Task<IActionResult> UpdateViewerCount(
        Guid liveStreamId,
        [FromBody] UpdateViewerCountRequest request,
        CancellationToken cancellationToken)
    {
        await _liveStreamService.UpdateViewerCountAsync(liveStreamId, request.ConcurrentViewers, cancellationToken);
        return NoContent();
    }
}

public class UpdateViewerCountRequest
{
    public long ConcurrentViewers { get; set; }
}
