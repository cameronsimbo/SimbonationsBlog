namespace Learn.Domain.Entities;

public class ExerciseAttempt : CreatedEntity<ExerciseAttempt>
{
    public Guid ExerciseId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserAnswer { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsPassing { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string? SuggestedCorrection { get; set; }
    public string? DetailedBreakdown { get; set; }
    public int AttemptNumber { get; set; }
    public int TimeTakenSeconds { get; set; }
    public bool IsAudioSubmission { get; set; }
    public int XPEarned { get; set; }

    public Exercise Exercise { get; set; } = null!;

    public static ExerciseAttempt Create(
        Guid exerciseId,
        string userId,
        string userAnswer,
        int score,
        bool isPassing,
        string feedback,
        int attemptNumber,
        int timeTakenSeconds,
        bool isAudioSubmission = false,
        string? suggestedCorrection = null,
        string? detailedBreakdown = null)
    {
        return new ExerciseAttempt
        {
            ExerciseId = exerciseId,
            UserId = userId,
            UserAnswer = userAnswer,
            Score = score,
            IsPassing = isPassing,
            Feedback = feedback,
            AttemptNumber = attemptNumber,
            TimeTakenSeconds = timeTakenSeconds,
            IsAudioSubmission = isAudioSubmission,
            SuggestedCorrection = suggestedCorrection,
            DetailedBreakdown = detailedBreakdown
        };
    }
}
