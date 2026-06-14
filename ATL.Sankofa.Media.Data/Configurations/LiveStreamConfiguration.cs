using ATL.Sankofa.Media.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATL.Sankofa.Media.Data.Configurations;

public class LiveStreamConfiguration : IEntityTypeConfiguration<LiveStream>
{
    public void Configure(EntityTypeBuilder<LiveStream> builder)
    {
        builder.HasKey(ls => ls.Id);
        builder.Property(ls => ls.Title).HasMaxLength(200).IsRequired();
        builder.Property(ls => ls.Description).HasMaxLength(5000);
        builder.Property(ls => ls.CloudflareLiveInputId).HasMaxLength(256).IsRequired();
        builder.Property(ls => ls.RtmpsUrl).HasMaxLength(1024);
        builder.Property(ls => ls.RtmpsStreamKey).HasMaxLength(512);
        builder.Property(ls => ls.SrtUrl).HasMaxLength(1024);
        builder.Property(ls => ls.WebRtcUrl).HasMaxLength(1024);
        builder.Property(ls => ls.HlsPlaybackUrl).HasMaxLength(1024);
        builder.Property(ls => ls.ThumbnailUrl).HasMaxLength(1024);
        builder.Property(ls => ls.TicketPrice).HasColumnType("decimal(10,2)");

        builder.HasIndex(ls => ls.CloudflareLiveInputId).IsUnique();
        builder.HasIndex(ls => ls.Status);
        builder.HasIndex(ls => ls.ScheduledStartTime);
    }
}
