using System.Collections.ObjectModel;
using ATL.Sankofa.Media.Business.Interfaces;
using ATL.Sankofa.Media.Business.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ATL.Sankofa.Media.UI.Maui.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IVideoService _videoService;
    private int _currentPage = 1;
    private const int PageSize = 9;
    private int _totalCount;

    public HomeViewModel(IVideoService videoService)
    {
        _videoService = videoService;
        Categories =
        [
            "All", "Gaming", "Music", "Travel", "Cooking", "Nature", "Space"
        ];
        SelectedCategory = "All";
    }

    [ObservableProperty]
    private VideoDto? _featuredVideo;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<VideoDto> Videos { get; } = [];
    public ObservableCollection<VideoDto> RecentVideos { get; } = [];
    public List<string> Categories { get; }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            _currentPage = 1;
            var category = SelectedCategory == "All" ? null : SelectedCategory;
            var result = await _videoService.GetPublicFeedAsync(_currentPage, PageSize, category);
            _totalCount = result.TotalCount;

            Videos.Clear();
            foreach (var video in result.Videos)
                Videos.Add(video);

            if (Videos.Count > 0)
                FeaturedVideo = Videos[0];

            await LoadRecentAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load: {ex.GetType().Name}: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[HomeViewModel] {ErrorMessage}");
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsLoading || Videos.Count >= _totalCount) return;
        IsLoading = true;

        try
        {
            _currentPage++;
            var category = SelectedCategory == "All" ? null : SelectedCategory;
            var result = await _videoService.GetPublicFeedAsync(_currentPage, PageSize, category);

            foreach (var video in result.Videos)
                Videos.Add(video);
        }
        catch
        {
            _currentPage--;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SelectCategoryAsync(string category)
    {
        SelectedCategory = category;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task NavigateToVideoAsync(VideoDto video)
    {
        await Shell.Current.GoToAsync($"watch?id={video.Id}");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadDataAsync();
    }

    private async Task LoadRecentAsync()
    {
        try
        {
            var result = await _videoService.GetPublicFeedAsync(1, 6);
            RecentVideos.Clear();
            foreach (var video in result.Videos.OrderByDescending(v => v.PublishedAt).Take(6))
                RecentVideos.Add(video);
        }
        catch { }
    }

    public static string FormatViewCount(long views) => views switch
    {
        >= 1_000_000 => $"{views / 1_000_000.0:0.#}M",
        >= 1_000 => $"{views / 1_000.0:0.#}K",
        _ => views.ToString()
    };

    public static string FormatTimeAgo(DateTime? date)
    {
        if (date == null) return "";
        var diff = DateTime.UtcNow - date.Value;
        return diff.TotalDays switch
        {
            >= 365 => $"{(int)(diff.TotalDays / 365)}y ago",
            >= 30 => $"{(int)(diff.TotalDays / 30)}mo ago",
            >= 7 => $"{(int)(diff.TotalDays / 7)}w ago",
            >= 1 => $"{(int)diff.TotalDays}d ago",
            _ when diff.TotalHours >= 1 => $"{(int)diff.TotalHours}h ago",
            _ => "Just now"
        };
    }

    public static string FormatDuration(TimeSpan? d) =>
        d == null ? "" :
        d.Value.TotalHours >= 1 ? d.Value.ToString(@"h\:mm\:ss") : d.Value.ToString(@"m\:ss");

    public static string GetChannelInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
            : name[..Math.Min(2, name.Length)].ToUpper();
    }
}
