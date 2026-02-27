using Learn.Domain.Enums;

namespace Learn.Domain.Entities;

public class QuestionBankItem : CreatedEntity<QuestionBankItem>
{
    public string UserId { get; set; } = string.Empty;
    public SubjectDomain SubjectDomain { get; set; }
    public ExerciseType ExerciseType { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string ReferenceAnswer { get; set; } = string.Empty;
    public string? Hints { get; set; }
    public int UsageCount { get; set; }

    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();

    public static QuestionBankItem Create(
        string userId,
        SubjectDomain subjectDomain,
        ExerciseType exerciseType,
        DifficultyLevel difficultyLevel,
        string prompt,
        string referenceAnswer,
        string? context = null,
        string? hints = null)
    {
        return new QuestionBankItem
        {
            UserId = userId,
            SubjectDomain = subjectDomain,
            ExerciseType = exerciseType,
            DifficultyLevel = difficultyLevel,
            Prompt = prompt,
            ReferenceAnswer = referenceAnswer,
            Context = context,
            Hints = hints,
            UsageCount = 0
        };
    }

    public void IncrementUsage()
    {
        UsageCount++;
    }
}
