using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATL.Sankofa.Media.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelsController : ControllerBase
{
    private readonly IChannelService _channelService;

    public ChannelsController(IChannelService channelService)
    {
        _channelService = channelService;
    }

    [HttpGet("{channelId:guid}")]
    public async Task<ActionResult<ChannelDto>> GetChannel(Guid channelId, CancellationToken cancellationToken)
    {
        var channel = await _channelService.GetChannelAsync(channelId, cancellationToken);
        if (channel == null) return NotFound();
        return Ok(channel);
    }

    [HttpGet("handle/{handle}")]
    public async Task<ActionResult<ChannelDto>> GetChannelByHandle(string handle, CancellationToken cancellationToken)
    {
        var channel = await _channelService.GetChannelByHandleAsync(handle, cancellationToken);
        if (channel == null) return NotFound();
        return Ok(channel);
    }

    [HttpGet("handle/{handle}/available")]
    public async Task<ActionResult<bool>> IsHandleAvailable(string handle, CancellationToken cancellationToken)
    {
        var available = await _channelService.IsHandleAvailableAsync(handle, cancellationToken);
        return Ok(available);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ChannelDto>> CreateChannel(
        [FromBody] CreateChannelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _channelService.CreateChannelAsync(request, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{channelId:guid}")]
    public async Task<ActionResult<ChannelDto>> UpdateChannel(
        Guid channelId,
        [FromBody] UpdateChannelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _channelService.UpdateChannelAsync(channelId, request, cancellationToken);
        return Ok(result);
    }
}
