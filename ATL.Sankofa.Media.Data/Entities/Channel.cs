namespace ATL.Sankofa.Media.Data.Entities;

public class Channel
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BannerUrl { get; set; }
    public string? AvatarUrl { get; set; }
    public string Handle { get; set; } = string.Empty;
    public int SubscriberCount { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Video> Videos { get; set; } = new List<Video>();
    public ICollection<LiveStream> LiveStreams { get; set; } = new List<LiveStream>();
    public ICollection<Subscription> Subscribers { get; set; } = new List<Subscription>();
}
