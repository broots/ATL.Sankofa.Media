using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using ATL.Sankofa.Media.Data.Entities;
using ATL.Sankofa.Media.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ATL.Sankofa.Media.Business.Services;

public class ChannelService : IChannelService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChannelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChannelDto> CreateChannelAsync(CreateChannelRequest request, CancellationToken cancellationToken = default)
    {
        var handleAvailable = await IsHandleAvailableAsync(request.Handle, cancellationToken);
        if (!handleAvailable)
        {
            throw new InvalidOperationException($"Handle '@{request.Handle}' is already taken.");
        }

        var channel = new Channel
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name,
            Description = request.Description,
            Handle = request.Handle
        };

        await _unitOfWork.Repository<Channel>().AddAsync(channel, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetChannelAsync(channel.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to create channel.");
    }

    public async Task<ChannelDto?> GetChannelAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        var channel = await _unitOfWork.Repository<Channel>().Query()
            .Include(c => c.Videos)
            .FirstOrDefaultAsync(c => c.Id == channelId, cancellationToken);

        if (channel == null) return null;

        return MapToDto(channel);
    }

    public async Task<ChannelDto?> GetChannelByHandleAsync(string handle, CancellationToken cancellationToken = default)
    {
        var channel = await _unitOfWork.Repository<Channel>().Query()
            .Include(c => c.Videos)
            .FirstOrDefaultAsync(c => c.Handle == handle, cancellationToken);

        if (channel == null) return null;

        return MapToDto(channel);
    }

    public async Task<ChannelDto> UpdateChannelAsync(Guid channelId, UpdateChannelRequest request, CancellationToken cancellationToken = default)
    {
        var channel = await _unitOfWork.Repository<Channel>().GetByIdAsync(channelId, cancellationToken)
            ?? throw new InvalidOperationException("Channel not found.");

        if (request.Name != null) channel.Name = request.Name;
        if (request.Description != null) channel.Description = request.Description;
        if (request.BannerUrl != null) channel.BannerUrl = request.BannerUrl;
        if (request.AvatarUrl != null) channel.AvatarUrl = request.AvatarUrl;
        channel.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Channel>().Update(channel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetChannelAsync(channel.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to update channel.");
    }

    public async Task<bool> IsHandleAvailableAsync(string handle, CancellationToken cancellationToken = default)
    {
        return !await _unitOfWork.Repository<Channel>().AnyAsync(
            c => c.Handle == handle, cancellationToken);
    }

    public async Task IncrementSubscriberCountAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        var channel = await _unitOfWork.Repository<Channel>().GetByIdAsync(channelId, cancellationToken);
        if (channel == null) return;

        channel.SubscriberCount++;
        _unitOfWork.Repository<Channel>().Update(channel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DecrementSubscriberCountAsync(Guid channelId, CancellationToken cancellationToken = default)
    {
        var channel = await _unitOfWork.Repository<Channel>().GetByIdAsync(channelId, cancellationToken);
        if (channel == null) return;

        if (channel.SubscriberCount > 0)
            channel.SubscriberCount--;

        _unitOfWork.Repository<Channel>().Update(channel);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ChannelDto MapToDto(Channel channel) => new()
    {
        Id = channel.Id,
        UserId = channel.UserId,
        Name = channel.Name,
        Description = channel.Description,
        BannerUrl = channel.BannerUrl,
        AvatarUrl = channel.AvatarUrl,
        Handle = channel.Handle,
        SubscriberCount = channel.SubscriberCount,
        IsVerified = channel.IsVerified,
        VideoCount = channel.Videos?.Count ?? 0,
        CreatedAt = channel.CreatedAt
    };
}
