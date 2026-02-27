using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class UserStreakConfiguration : IEntityTypeConfiguration<UserStreak>
{
    public void Configure(EntityTypeBuilder<UserStreak> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(e => e.UserId)
            .IsUnique();
    }
}
