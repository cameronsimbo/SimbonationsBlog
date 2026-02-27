using Learn.Application.Common.Interfaces;
using Learn.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Learn.Infrastructure.Persistence;

public class LearnDbContext : IdentityDbContext<IdentityUser>, ILearnDbContext
{
    public LearnDbContext(DbContextOptions<LearnDbContext> options) : base(options)
    {
    }

    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseAttempt> ExerciseAttempts => Set<ExerciseAttempt>();
    public DbSet<QuestionBankItem> QuestionBankItems => Set<QuestionBankItem>();
    public DbSet<UserProgress> UserProgress => Set<UserProgress>();
    public DbSet<UserStreak> UserStreaks => Set<UserStreak>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();
    public DbSet<DailyLesson> DailyLessons => Set<DailyLesson>();
    public DbSet<ExerciseVote> ExerciseVotes => Set<ExerciseVote>();
    public DbSet<ReviewItem> ReviewItems => Set<ReviewItem>();
    public DbSet<UserTopicEnrollment> UserTopicEnrollments => Set<UserTopicEnrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.Entity is ICreatedEntity auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedDate = now;
                    auditable.ModifiedDate = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.ModifiedDate = now;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
