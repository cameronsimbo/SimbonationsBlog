namespace Learn.Domain.Entities;

public class Lesson : CreatedEntity<Lesson>
{
    public Guid UnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int ExerciseCount { get; set; }
    public int EstimatedMinutes { get; set; }
    public bool HasGeneratedExercises { get; set; }
    public string? GenerationContext { get; set; }

    public Unit Unit { get; set; } = null!;
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();

    public static Lesson Create(
        Guid unitId,
        string name,
        string description,
        int orderIndex,
        int estimatedMinutes = 5)
    {
        return new Lesson
        {
            UnitId = unitId,
            Name = name,
            Description = description,
            OrderIndex = orderIndex,
            ExerciseCount = 0,
            EstimatedMinutes = estimatedMinutes
        };
    }
}
