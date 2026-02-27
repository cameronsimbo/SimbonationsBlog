using Learn.Domain.Entities;
using Learn.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Learn.Domain.Tests;

public class ExerciseVoteTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        Guid exerciseId = Guid.NewGuid();
        string userId = "user-1";

        ExerciseVote vote = ExerciseVote.Create(exerciseId, userId, true);

        vote.ExerciseId.Should().Be(exerciseId);
        vote.UserId.Should().Be(userId);
        vote.IsUpvote.Should().BeTrue();
    }

    [Fact]
    public void ChangeVote_ShouldFlipDirection()
    {
        ExerciseVote vote = ExerciseVote.Create(Guid.NewGuid(), "user-1", true);

        vote.ChangeVote(false);

        vote.IsUpvote.Should().BeFalse();
    }
}

public class ExerciseVotingTests
{
    private static Exercise CreateExercise()
    {
        return Exercise.Create(
            Guid.NewGuid(), 0, ExerciseType.Explanation,
            DifficultyLevel.Beginner, "Test", "Answer");
    }

    [Fact]
    public void VoteScore_ShouldBeUpvotesMinusDownvotes()
    {
        Exercise exercise = CreateExercise();
        exercise.IncrementUpvote();
        exercise.IncrementUpvote();
        exercise.IncrementDownvote();

        exercise.VoteScore.Should().Be(1);
    }

    [Fact]
    public void IsHidden_ShouldBeTrueWhenVoteScoreBelowThreshold()
    {
        Exercise exercise = CreateExercise();
        exercise.IncrementDownvote();
        exercise.IncrementDownvote();
        exercise.IncrementDownvote();
        exercise.IncrementDownvote(); // -4

        exercise.IsHidden.Should().BeTrue();
    }

    [Fact]
    public void IsHidden_ShouldBeFalseWhenVoteScoreAboveThreshold()
    {
        Exercise exercise = CreateExercise();
        exercise.IncrementDownvote();
        exercise.IncrementDownvote();
        exercise.IncrementDownvote(); // -3

        exercise.IsHidden.Should().BeFalse();
    }

    [Fact]
    public void DecrementUpvote_ShouldNotGoBelowZero()
    {
        Exercise exercise = CreateExercise();
        exercise.DecrementUpvote();

        exercise.UpvoteCount.Should().Be(0);
    }

    [Fact]
    public void DecrementDownvote_ShouldNotGoBelowZero()
    {
        Exercise exercise = CreateExercise();
        exercise.DecrementDownvote();

        exercise.DownvoteCount.Should().Be(0);
    }
}
