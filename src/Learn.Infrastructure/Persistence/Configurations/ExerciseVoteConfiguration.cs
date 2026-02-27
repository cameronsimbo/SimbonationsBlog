using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class ExerciseVoteConfiguration : IEntityTypeConfiguration<ExerciseVote>
{
    public void Configure(EntityTypeBuilder<ExerciseVote> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(v => new { v.ExerciseId, v.UserId })
            .IsUnique();

        builder.HasOne(v => v.Exercise)
            .WithMany(e => e.Votes)
            .HasForeignKey(v => v.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
