using ATL.Sankofa.Media.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATL.Sankofa.Media.Data.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Title).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Description).HasMaxLength(5000);
        builder.Property(v => v.CloudflareVideoId).HasMaxLength(256).IsRequired();
        builder.Property(v => v.CloudflareVideoUrl).HasMaxLength(1024);
        builder.Property(v => v.ThumbnailUrl).HasMaxLength(1024);
        builder.Property(v => v.HlsPlaybackUrl).HasMaxLength(1024);
        builder.Property(v => v.DashPlaybackUrl).HasMaxLength(1024);
        builder.Property(v => v.Tags).HasMaxLength(1000);
        builder.Property(v => v.Category).HasMaxLength(100);
        builder.Property(v => v.PurchasePrice).HasColumnType("decimal(10,2)");

        builder.HasIndex(v => v.CloudflareVideoId).IsUnique();
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.Visibility);
        builder.HasIndex(v => v.PublishedAt);
    }
}
