using Learn.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Learn.Application.Common.Interfaces;

public interface ILearnDbContext : IDisposable
{
    DbSet<Topic> Topics { get; }
    DbSet<Unit> Units { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<ExerciseAttempt> ExerciseAttempts { get; }
    DbSet<QuestionBankItem> QuestionBankItems { get; }
    DbSet<UserProgress> UserProgress { get; }
    DbSet<UserStreak> UserStreaks { get; }
    DbSet<Achievement> Achievements { get; }
    DbSet<UserAchievement> UserAchievements { get; }
    DbSet<LeaderboardEntry> LeaderboardEntries { get; }
    DbSet<DailyLesson> DailyLessons { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
