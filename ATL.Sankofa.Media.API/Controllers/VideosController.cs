using System.Security.Claims;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATL.Sankofa.Media.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController : ControllerBase
{
    private readonly IVideoService _videoService;

    public VideosController(IVideoService videoService)
    {
        _videoService = videoService;
    }

    [HttpGet("feed")]
    public async Task<ActionResult<VideoListResponse>> GetPublicFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _videoService.GetPublicFeedAsync(page, pageSize, category, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{videoId:guid}")]
    public async Task<ActionResult<VideoDto>> GetVideo(Guid videoId, CancellationToken cancellationToken)
    {
        var video = await _videoService.GetVideoAsync(videoId, cancellationToken);
        if (video == null) return NotFound();
        return Ok(video);
    }

    [HttpGet("channel/{channelId:guid}")]
    public async Task<ActionResult<VideoListResponse>> GetChannelVideos(
        Guid channelId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _videoService.GetVideosAsync(channelId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("upload")]
    public async Task<ActionResult<VideoUploadResponse>> InitiateUpload(
        [FromBody] VideoUploadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _videoService.InitiateUploadAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{videoId:guid}")]
    public async Task<ActionResult<VideoDto>> UpdateVideo(
        Guid videoId,
        [FromBody] VideoUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _videoService.UpdateVideoAsync(videoId, request.Title, request.Description, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{videoId:guid}/publish")]
    public async Task<IActionResult> PublishVideo(Guid videoId, CancellationToken cancellationToken)
    {
        await _videoService.PublishVideoAsync(videoId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{videoId:guid}")]
    public async Task<IActionResult> DeleteVideo(Guid videoId, CancellationToken cancellationToken)
    {
        await _videoService.DeleteVideoAsync(videoId, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("{videoId:guid}/playback-url")]
    public async Task<ActionResult<PlaybackUrlResponse>> GetPlaybackUrl(Guid videoId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var url = await _videoService.GetPlaybackUrlAsync(videoId, userId, cancellationToken);
        return Ok(new PlaybackUrlResponse { Url = url });
    }

    [HttpPost("{videoId:guid}/view")]
    public async Task<IActionResult> IncrementViewCount(Guid videoId, CancellationToken cancellationToken)
    {
        await _videoService.IncrementViewCountAsync(videoId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{videoId:guid}/sync-status")]
    public async Task<IActionResult> SyncVideoStatus(Guid videoId, CancellationToken cancellationToken)
    {
        await _videoService.SyncVideoStatusAsync(videoId, cancellationToken);
        return NoContent();
    }
}

public class VideoUpdateRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public class PlaybackUrlResponse
{
    public string? Url { get; set; }
}
