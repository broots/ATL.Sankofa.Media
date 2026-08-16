using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ATL.Sankofa.Media.UI.Maui.ViewModels;

[QueryProperty(nameof(VideoId), "id")]
public partial class WatchViewModel : ObservableObject
{
    private readonly IVideoService _videoService;

    public WatchViewModel(IVideoService videoService)
    {
        _videoService = videoService;
    }

    [ObservableProperty]
    private string? _videoId;

    [ObservableProperty]
    private VideoDto? _video;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _playbackSource;

    [ObservableProperty]
    private string _channelInitial = "?";

    [ObservableProperty]
    private bool _hasDescription;

    partial void OnVideoIdChanged(string? value)
    {
        if (Guid.TryParse(value, out _))
            LoadVideoCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadVideoAsync()
    {
        if (string.IsNullOrEmpty(VideoId) || !Guid.TryParse(VideoId, out var id))
            return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Video = await _videoService.GetVideoAsync(id);
            if (Video == null)
            {
                ErrorMessage = "Video not found.";
                return;
            }

            await _videoService.IncrementViewCountAsync(id);

            PlaybackSource = Video.HlsPlaybackUrl ?? Video.DashPlaybackUrl;
            HasDescription = !string.IsNullOrWhiteSpace(Video.Description);
            ChannelInitial = string.IsNullOrWhiteSpace(Video.ChannelName)
                ? "?"
                : Video.ChannelName[..1].ToUpperInvariant();

            if (string.IsNullOrEmpty(PlaybackSource))
                ErrorMessage = "No playback URL available for this video.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load video: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[WatchViewModel] {ErrorMessage}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}