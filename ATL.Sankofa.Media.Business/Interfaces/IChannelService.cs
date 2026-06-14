using ATL.Sankofa.Media.Business.Models;

namespace ATL.Sankofa.Media.Business.Interfaces;

public interface IChannelService
{
    Task<ChannelDto> CreateChannelAsync(CreateChannelRequest request, CancellationToken cancellationToken = default);
    Task<ChannelDto?> GetChannelAsync(Guid channelId, CancellationToken cancellationToken = default);
    Task<ChannelDto?> GetChannelByHandleAsync(string handle, CancellationToken cancellationToken = default);
    Task<ChannelDto> UpdateChannelAsync(Guid channelId, UpdateChannelRequest request, CancellationToken cancellationToken = default);
    Task<bool> IsHandleAvailableAsync(string handle, CancellationToken cancellationToken = default);
    Task IncrementSubscriberCountAsync(Guid channelId, CancellationToken cancellationToken = default);
    Task DecrementSubscriberCountAsync(Guid channelId, CancellationToken cancellationToken = default);
}
