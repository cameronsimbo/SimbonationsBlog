using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(e => new { e.UserId, e.AchievementId })
            .IsUnique();

        builder.HasOne(e => e.Achievement)
            .WithMany(a => a.UserAchievements)
            .HasForeignKey(e => e.AchievementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
