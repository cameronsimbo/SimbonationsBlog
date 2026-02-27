using Learn.Domain.Enums;

namespace Learn.Domain.Entities;

public class Topic : CreatedEntity<Topic>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SubjectDomain SubjectDomain { get; set; }
    public string? IconUrl { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public bool IsPublished { get; set; }
    public int TotalUnits { get; set; }
    public string? KeyConcepts { get; set; }
    public string? GenerationGuidance { get; set; }

    public ICollection<Unit> Units { get; set; } = new List<Unit>();

    public static Topic Create(
        string name,
        string description,
        SubjectDomain subjectDomain,
        DifficultyLevel difficultyLevel,
        string? iconUrl = null)
    {
        return new Topic
        {
            Name = name,
            Description = description,
            SubjectDomain = subjectDomain,
            DifficultyLevel = difficultyLevel,
            IconUrl = iconUrl,
            IsPublished = false,
            TotalUnits = 0
        };
    }

    public void Publish()
    {
        IsPublished = true;
    }

    public void Unpublish()
    {
        IsPublished = false;
    }
}
