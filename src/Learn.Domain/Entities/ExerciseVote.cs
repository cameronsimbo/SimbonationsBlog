namespace Learn.Domain.Entities;

public class ExerciseVote : CreatedEntity<ExerciseVote>
{
    public Guid ExerciseId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsUpvote { get; set; }

    public Exercise Exercise { get; set; } = null!;

    public static ExerciseVote Create(Guid exerciseId, string userId, bool isUpvote)
    {
        return new ExerciseVote
        {
            ExerciseId = exerciseId,
            UserId = userId,
            IsUpvote = isUpvote
        };
    }

    public void ChangeVote(bool isUpvote)
    {
        IsUpvote = isUpvote;
    }
}
