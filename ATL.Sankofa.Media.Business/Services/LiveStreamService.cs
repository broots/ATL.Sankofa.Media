using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Business.Models.Cloudflare;
using ATL.Sankofa.Media.Data.Entities;
using ATL.Sankofa.Media.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATL.Sankofa.Media.Business.Services;

public class LiveStreamService : ILiveStreamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICloudflareStreamClient _cloudflareClient;
    private readonly IPaywallService _paywallService;

    public LiveStreamService(IUnitOfWork unitOfWork, ICloudflareStreamClient cloudflareClient, IPaywallService paywallService)
    {
        _unitOfWork = unitOfWork;
        _cloudflareClient = cloudflareClient;
        _paywallService = paywallService;
    }

    public async Task<LiveStreamDto> CreateLiveStreamAsync(CreateLiveStreamRequest request, CancellationToken cancellationToken = default)
    {
        var createInput = new CreateLiveInputRequest
        {
            Meta = new Dictionary<string, string>
            {
                ["title"] = request.Title,
                ["channelId"] = request.ChannelId.ToString()
            },
            Recording = request.RecordStream ? new CloudflareRecording { Mode = "automatic" } : null
        };

        var liveInput = await _cloudflareClient.CreateLiveInputAsync(createInput, cancellationToken);

        var liveStream = new Data.Entities.LiveStream
        {
            Id = Guid.NewGuid(),
            ChannelId = request.ChannelId,
            Title = request.Title,
            Description = request.Description,
            CloudflareLiveInputId = liveInput.Uid,
            RtmpsUrl = liveInput.Rtmps?.Url,
            RtmpsStreamKey = liveInput.Rtmps?.StreamKey,
            SrtUrl = liveInput.Srt?.Url,
            WebRtcUrl = liveInput.WebRtc?.Url,
            RequiredAccessTier = request.RequiredAccessTier,
            TicketPrice = request.TicketPrice,
            ScheduledStartTime = request.ScheduledStartTime,
            RecordStream = request.RecordStream,
            Status = request.ScheduledStartTime.HasValue ? LiveStreamStatus.Scheduled : LiveStreamStatus.Idle
        };

        await _unitOfWork.Repository<Data.Entities.LiveStream>().AddAsync(liveStream, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetLiveStreamAsync(liveStream.Id, cancellationToken) ?? throw new InvalidOperationException("Failed to create live stream.");
    }

    public async Task<LiveStreamDto?> GetLiveStreamAsync(Guid liveStreamId, CancellationToken cancellationToken = default)
    {
        var liveStream = await _unitOfWork.Repository<Data.Entities.LiveStream>().Query()
            .Include(ls => ls.Channel)
            .FirstOrDefaultAsync(ls => ls.Id == liveStreamId, cancellationToken);

        if (liveStream == null) return null;

        return MapToDto(liveStream);
    }

    public async Task<LiveStreamListResponse> GetLiveStreamsAsync(Guid channelId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Data.Entities.LiveStream>().Query()
            .Include(ls => ls.Channel)
            .Where(ls => ls.ChannelId == channelId)
            .OrderByDescending(ls => ls.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var liveStreams = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new LiveStreamListResponse
        {
            LiveStreams = liveStreams.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<LiveStreamListResponse> GetActiveLiveStreamsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Data.Entities.LiveStream>().Query()
            .Include(ls => ls.Channel)
            .Where(ls => ls.Status == LiveStreamStatus.Live || ls.Status == LiveStreamStatus.Scheduled)
            .OrderByDescending(ls => ls.Status == LiveStreamStatus.Live)
            .ThenByDescending(ls => ls.ConcurrentViewers);

        var totalCount = await query.CountAsync(cancellationToken);
        var liveStreams = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new LiveStreamListResponse
        {
            LiveStreams = liveStreams.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<string?> GetPlaybackUrlAsync(Guid liveStreamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var liveStream = await _unitOfWork.Repository<Data.Entities.LiveStream>().GetByIdAsync(liveStreamId, cancellationToken)
            ?? throw new InvalidOperationException("Live stream not found.");

        var accessCheck = await _paywallService.CheckLiveStreamAccessAsync(userId, liveStreamId, cancellationToken);
        if (!accessCheck.HasAccess)
            return null;

        return liveStream.HlsPlaybackUrl;
    }

    public async Task EndLiveStreamAsync(Guid liveStreamId, CancellationToken cancellationToken = default)
    {
        var liveStream = await _unitOfWork.Repository<Data.Entities.LiveStream>().GetByIdAsync(liveStreamId, cancellationToken)
            ?? throw new InvalidOperationException("Live stream not found.");

        liveStream.Status = LiveStreamStatus.Ended;
        liveStream.EndTime = DateTime.UtcNow;
        liveStream.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Data.Entities.LiveStream>().Update(liveStream);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteLiveStreamAsync(Guid liveStreamId, CancellationToken cancellationToken = default)
    {
        var liveStream = await _unitOfWork.Repository<Data.Entities.LiveStream>().GetByIdAsync(liveStreamId, cancellationToken)
            ?? throw new InvalidOperationException("Live stream not found.");

        await _cloudflareClient.DeleteLiveInputAsync(liveStream.CloudflareLiveInputId, cancellationToken);

        _unitOfWork.Repository<Data.Entities.LiveStream>().Remove(liveStream);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateViewerCountAsync(Guid liveStreamId, long concurrentViewers, CancellationToken cancellationToken = default)
    {
        var liveStream = await _unitOfWork.Repository<Data.Entities.LiveStream>().GetByIdAsync(liveStreamId, cancellationToken);
        if (liveStream == null) return;

        liveStream.ConcurrentViewers = concurrentViewers;
        if (concurrentViewers > liveStream.TotalViewers)
            liveStream.TotalViewers = concurrentViewers;

        _unitOfWork.Repository<Data.Entities.LiveStream>().Update(liveStream);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static LiveStreamDto MapToDto(Data.Entities.LiveStream liveStream) => new()
    {
        Id = liveStream.Id,
        ChannelId = liveStream.ChannelId,
        ChannelName = liveStream.Channel?.Name ?? string.Empty,
        Title = liveStream.Title,
        Description = liveStream.Description,
        ThumbnailUrl = liveStream.ThumbnailUrl,
        HlsPlaybackUrl = liveStream.HlsPlaybackUrl,
        RtmpsUrl = liveStream.RtmpsUrl,
        RtmpsStreamKey = liveStream.RtmpsStreamKey,
        SrtUrl = liveStream.SrtUrl,
        WebRtcUrl = liveStream.WebRtcUrl,
        Status = liveStream.Status,
        RequiredAccessTier = liveStream.RequiredAccessTier,
        TicketPrice = liveStream.TicketPrice,
        ConcurrentViewers = liveStream.ConcurrentViewers,
        ScheduledStartTime = liveStream.ScheduledStartTime,
        ActualStartTime = liveStream.ActualStartTime,
        CreatedAt = liveStream.CreatedAt
    };
}
