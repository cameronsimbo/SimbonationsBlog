using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learn.Infrastructure.Persistence.Configurations;

public class UserTopicEnrollmentConfiguration : IEntityTypeConfiguration<UserTopicEnrollment>
{
    public void Configure(EntityTypeBuilder<UserTopicEnrollment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasIndex(e => new { e.UserId, e.TopicId })
            .IsUnique();

        builder.HasIndex(e => new { e.UserId, e.IsActive });

        builder.HasOne(e => e.Topic)
            .WithMany()
            .HasForeignKey(e => e.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
