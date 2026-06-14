using ATL.Sankofa.Media.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATL.Sankofa.Media.Data.Configurations;

public class WatchHistoryConfiguration : IEntityTypeConfiguration<WatchHistory>
{
    public void Configure(EntityTypeBuilder<WatchHistory> builder)
    {
        builder.HasKey(wh => wh.Id);

        builder.HasIndex(wh => new { wh.UserId, wh.VideoId }).IsUnique();
        builder.HasIndex(wh => wh.LastWatchedAt);

        builder.HasOne(wh => wh.User)
            .WithMany(u => u.WatchHistories)
            .HasForeignKey(wh => wh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wh => wh.Video)
            .WithMany(v => v.WatchHistories)
            .HasForeignKey(wh => wh.VideoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
