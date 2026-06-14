namespace ATL.Sankofa.Media.Data.Entities;

public class WatchHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid VideoId { get; set; }

    public TimeSpan WatchedDuration { get; set; }
    public TimeSpan? TotalDuration { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;
    public int WatchCount { get; set; } = 1;

    // Navigation properties
    public User User { get; set; } = null!;
    public Video Video { get; set; } = null!;
}
