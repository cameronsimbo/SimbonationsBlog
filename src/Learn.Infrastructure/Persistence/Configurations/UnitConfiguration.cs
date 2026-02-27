using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(u => u.Description)
            .HasMaxLength(2000);

        builder.HasIndex(u => new { u.TopicId, u.OrderIndex });

        builder.HasMany(u => u.Lessons)
            .WithOne(l => l.Unit)
            .HasForeignKey(l => l.UnitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
