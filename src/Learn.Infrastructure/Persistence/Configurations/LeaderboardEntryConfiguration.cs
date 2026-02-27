using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class LeaderboardEntryConfiguration : IEntityTypeConfiguration<LeaderboardEntry>
{
    public void Configure(EntityTypeBuilder<LeaderboardEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(e => new { e.WeekStartDate, e.WeeklyXP })
            .IsDescending(false, true);

        builder.HasIndex(e => new { e.UserId, e.WeekStartDate })
            .IsUnique();
    }
}
