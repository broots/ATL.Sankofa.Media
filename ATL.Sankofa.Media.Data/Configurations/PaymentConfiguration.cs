using ATL.Sankofa.Media.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATL.Sankofa.Media.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasColumnType("decimal(10,2)");
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.ExternalPaymentId).HasMaxLength(256);
        builder.Property(p => p.PaymentMethod).HasMaxLength(50);
        builder.Property(p => p.Description).HasMaxLength(500);

        builder.HasIndex(p => p.ExternalPaymentId);
        builder.HasIndex(p => p.Status);

        builder.HasOne(p => p.User)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Video)
            .WithMany(v => v.Payments)
            .HasForeignKey(p => p.VideoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
