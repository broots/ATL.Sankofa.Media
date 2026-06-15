using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Business.Models.Cloudflare;
using ATL.Sankofa.Media.Data.Entities;
using ATL.Sankofa.Media.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATL.Sankofa.Media.Business.Services;

public class VideoService : IVideoService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICloudflareStreamClient _cloudflareClient;
    private readonly IPaywallService _paywallService;

    public VideoService(IUnitOfWork unitOfWork, ICloudflareStreamClient cloudflareClient, IPaywallService paywallService)
    {
        _unitOfWork = unitOfWork;
        _cloudflareClient = cloudflareClient;
        _paywallService = paywallService;
    }

    public async Task<VideoUploadResponse> InitiateUploadAsync(VideoUploadRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ChannelId == Guid.Empty)
            request.ChannelId = null;

        if (request.ChannelId.HasValue)
        {
            var channelExists = await _unitOfWork.Repository<Channel>().Query()
                .AnyAsync(c => c.Id == request.ChannelId.Value, cancellationToken);

            if (!channelExists)
                throw new InvalidOperationException($"Channel '{request.ChannelId}' not found.");
        }

        var meta = new Dictionary<string, string>
        {
            ["title"] = request.Title
        };

        if (request.ChannelId.HasValue)
            meta["channelId"] = request.ChannelId.Value.ToString();

        var uploadRequest = new DirectUploadRequest
        {
            MaxDurationSeconds = request.MaxDurationSeconds,
            RequireSignedUrls = true,
            Meta = meta
        };

        var uploadResult = await _cloudflareClient.CreateDirectUploadAsync(uploadRequest, cancellationToken);

        var video = new Video
        {
            Id = Guid.NewGuid(),
            ChannelId = request.ChannelId,
            Title = request.Title,
            Description = request.Description,
            CloudflareVideoId = uploadResult.Uid,
            Tags = request.Tags,
            Category = request.Category,
            Visibility = request.Visibility,
            RequiredAccessTier = request.RequiredAccessTier,
            PurchasePrice = request.PurchasePrice,
            Status = VideoStatus.Processing
        };

        await _unitOfWork.Repository<Video>().AddAsync(video, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VideoUploadResponse
        {
            VideoId = video.Id,
            CloudflareVideoId = uploadResult.Uid,
            UploadUrl = uploadResult.UploadUrl
        };
    }

    public async Task<VideoDto?> GetVideoAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Repository<Video>().Query()
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken);

        if (video == null) return null;

        return MapToDto(video);
    }

    public async Task<VideoListResponse> GetVideosAsync(Guid channelId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Video>().Query()
            .Include(v => v.Channel)
            .Where(v => v.ChannelId == channelId && v.Status == VideoStatus.Ready)
            .OrderByDescending(v => v.PublishedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var videos = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new VideoListResponse
        {
            Videos = videos.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<VideoListResponse> GetPublicFeedAsync(int page = 1, int pageSize = 20, string? category = null, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Video>().Query()
            .Include(v => v.Channel)
            .Where(v => v.Status == VideoStatus.Ready && v.Visibility == VideoVisibility.Public);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(v => v.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var videos = await query
            .OrderByDescending(v => v.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new VideoListResponse
        {
            Videos = videos.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<VideoDto> UpdateVideoAsync(Guid videoId, string? title, string? description, CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Repository<Video>().Query()
            .Include(v => v.Channel)
            .FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        if (title != null) video.Title = title;
        if (description != null) video.Description = description;
        video.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Video>().Update(video);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(video);
    }

    public async Task PublishVideoAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Repository<Video>().GetByIdAsync(videoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        video.Visibility = VideoVisibility.Public;
        video.PublishedAt = DateTime.UtcNow;
        video.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Video>().Update(video);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteVideoAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Repository<Video>().GetByIdAsync(videoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        await _cloudflareClient.DeleteVideoAsync(video.CloudflareVideoId, cancellationToken);

        video.Status = VideoStatus.Deleted;
        video.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Video>().Update(video);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> GetPlaybackUrlAsync(Guid videoId, Guid userId, CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Repository<Video>().GetByIdAsync(videoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        var accessCheck = await _paywallService.CheckVideoAccessAsync(userId, videoId, cancellationToken);
        if (!accessCheck.HasAccess)
            return null;

        var token = await _cloudflareClient.GenerateSignedTokenAsync(video.CloudflareVideoId);
        return $"{video.HlsPlaybackUrl}?token={token}";
    }

    public async Task IncrementViewCountAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Repository<Video>().GetByIdAsync(videoId, cancellationToken);
        if (video == null) return;

        video.ViewCount++;
        _unitOfWork.Repository<Video>().Update(video);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncVideoStatusAsync(Guid videoId, CancellationToken cancellationToken = default)
    {
        var video = await _unitOfWork.Repository<Video>().GetByIdAsync(videoId, cancellationToken)
            ?? throw new InvalidOperationException("Video not found.");

        var cloudflareVideo = await _cloudflareClient.GetVideoAsync(video.CloudflareVideoId, cancellationToken);
        if (cloudflareVideo == null) return;

        if (cloudflareVideo.ReadyToStream)
        {
            video.Status = VideoStatus.Ready;
            video.HlsPlaybackUrl = cloudflareVideo.Playback?.Hls;
            video.DashPlaybackUrl = cloudflareVideo.Playback?.Dash;
            video.ThumbnailUrl = cloudflareVideo.Thumbnail;
            if (cloudflareVideo.Duration.HasValue)
                video.Duration = TimeSpan.FromSeconds(cloudflareVideo.Duration.Value);
        }
        else if (cloudflareVideo.Status?.State == "error")
        {
            video.Status = VideoStatus.Failed;
        }

        video.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<Video>().Update(video);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static VideoDto MapToDto(Video video) => new()
    {
        Id = video.Id,
        ChannelId = video.ChannelId,
        ChannelName = video.Channel?.Name ?? string.Empty,
        Title = video.Title,
        Description = video.Description,
        ThumbnailUrl = video.ThumbnailUrl,
        HlsPlaybackUrl = video.HlsPlaybackUrl,
        DashPlaybackUrl = video.DashPlaybackUrl,
        Duration = video.Duration,
        ViewCount = video.ViewCount,
        LikeCount = video.LikeCount,
        Status = video.Status,
        Visibility = video.Visibility,
        RequiredAccessTier = video.RequiredAccessTier,
        PurchasePrice = video.PurchasePrice,
        Tags = video.Tags,
        Category = video.Category,
        PublishedAt = video.PublishedAt,
        CreatedAt = video.CreatedAt
    };
}
