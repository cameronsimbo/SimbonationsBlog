using Learn.Domain.Enums;

namespace Learn.Domain.Entities;

public class Exercise : CreatedEntity<Exercise>
{
    public Guid LessonId { get; set; }
    public int OrderIndex { get; set; }
    public ExerciseType ExerciseType { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string ReferenceAnswer { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Hints { get; set; }
    public int MaxScore { get; set; } = 100;
    public Guid? QuestionBankItemId { get; set; }
    public Guid? DailyLessonId { get; set; }
    public int UpvoteCount { get; set; }
    public int DownvoteCount { get; set; }

    public int VoteScore => UpvoteCount - DownvoteCount;
    public bool IsHidden => VoteScore < -3;

    public Lesson Lesson { get; set; } = null!;
    public QuestionBankItem? QuestionBankItem { get; set; }
    public DailyLesson? DailyLesson { get; set; }
    public ICollection<ExerciseAttempt> Attempts { get; set; } = new List<ExerciseAttempt>();
    public ICollection<ExerciseVote> Votes { get; set; } = new List<ExerciseVote>();

    public void IncrementUpvote() => UpvoteCount++;
    public void DecrementUpvote() => UpvoteCount = Math.Max(0, UpvoteCount - 1);
    public void IncrementDownvote() => DownvoteCount++;
    public void DecrementDownvote() => DownvoteCount = Math.Max(0, DownvoteCount - 1);

    public static Exercise Create(
        Guid lessonId,
        int orderIndex,
        ExerciseType exerciseType,
        DifficultyLevel difficultyLevel,
        string prompt,
        string referenceAnswer,
        string? context = null,
        string? audioUrl = null,
        string? hints = null,
        int maxScore = 100)
    {
        return new Exercise
        {
            LessonId = lessonId,
            OrderIndex = orderIndex,
            ExerciseType = exerciseType,
            DifficultyLevel = difficultyLevel,
            Prompt = prompt,
            ReferenceAnswer = referenceAnswer,
            Context = context,
            AudioUrl = audioUrl,
            Hints = hints,
            MaxScore = maxScore
        };
    }

    public bool RequiresAudio()
    {
        return ExerciseType == ExerciseType.ListeningComprehension
            || ExerciseType == ExerciseType.SpeakingResponse;
    }
}
