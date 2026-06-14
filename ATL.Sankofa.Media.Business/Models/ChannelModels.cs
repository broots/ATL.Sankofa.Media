namespace ATL.Sankofa.Media.Business.Models;

public class ChannelDto
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
    public int VideoCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateChannelRequest
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Handle { get; set; } = string.Empty;
}

public class UpdateChannelRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? BannerUrl { get; set; }
    public string? AvatarUrl { get; set; }
}
