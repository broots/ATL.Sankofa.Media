using ATL.Sankofa.Media.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATL.Sankofa.Media.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Currency).HasMaxLength(3).IsRequired();
        builder.Property(s => s.Price).HasColumnType("decimal(10,2)");
        builder.Property(s => s.ExternalSubscriptionId).HasMaxLength(256);

        builder.HasIndex(s => new { s.UserId, s.ChannelId, s.Status });

        builder.HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Channel)
            .WithMany(c => c.Subscribers)
            .HasForeignKey(s => s.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
