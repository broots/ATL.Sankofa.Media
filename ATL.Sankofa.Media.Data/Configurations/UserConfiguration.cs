using ATL.Sankofa.Media.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATL.Sankofa.Media.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.AvatarUrl).HasMaxLength(1024);
        builder.Property(u => u.Bio).HasMaxLength(1000);

        builder.HasOne(u => u.Channel)
            .WithOne(c => c.User)
            .HasForeignKey<Channel>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
