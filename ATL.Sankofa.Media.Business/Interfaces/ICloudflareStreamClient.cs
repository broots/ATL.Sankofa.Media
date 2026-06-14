using ATL.Sankofa.Media.Business.Models.Cloudflare;

namespace ATL.Sankofa.Media.Business.Interfaces;

public interface ICloudflareStreamClient
{
    Task<DirectUploadResult> CreateDirectUploadAsync(DirectUploadRequest request, CancellationToken cancellationToken = default);
    Task<CloudflareVideoResult?> GetVideoAsync(string videoId, CancellationToken cancellationToken = default);
    Task DeleteVideoAsync(string videoId, CancellationToken cancellationToken = default);
    Task<CloudflareLiveInputResult> CreateLiveInputAsync(CreateLiveInputRequest request, CancellationToken cancellationToken = default);
    Task<CloudflareLiveInputResult?> GetLiveInputAsync(string liveInputId, CancellationToken cancellationToken = default);
    Task DeleteLiveInputAsync(string liveInputId, CancellationToken cancellationToken = default);
    Task<string> GenerateSignedTokenAsync(string videoId, int lifetimeMinutes = 60);
}
