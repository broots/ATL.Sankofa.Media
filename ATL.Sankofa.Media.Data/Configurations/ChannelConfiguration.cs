using ATL.Sankofa.Media.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATL.Sankofa.Media.Data.Configurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.Handle).HasMaxLength(50).IsRequired();
        builder.Property(c => c.BannerUrl).HasMaxLength(1024);
        builder.Property(c => c.AvatarUrl).HasMaxLength(1024);

        builder.HasIndex(c => c.Handle).IsUnique();

        builder.HasMany(c => c.Videos)
            .WithOne(v => v.Channel)
            .HasForeignKey(v => v.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.LiveStreams)
            .WithOne(ls => ls.Channel)
            .HasForeignKey(ls => ls.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
