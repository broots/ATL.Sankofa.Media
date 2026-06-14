using ATL.Sankofa.Media.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ATL.Sankofa.Media.Data;

public class MediaDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public MediaDbContext(DbContextOptions<MediaDbContext> options) : base(options)
    {
    }

    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Video> Videos => Set<Video>();
    public DbSet<LiveStream> LiveStreams => Set<LiveStream>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<WatchHistory> WatchHistories => Set<WatchHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
    }
}
