namespace Learn.Domain.Entities;

public class Unit : CreatedEntity<Unit>
{
    public Guid TopicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int TotalLessons { get; set; }

    public Topic Topic { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public static Unit Create(
        Guid topicId,
        string name,
        string description,
        int orderIndex)
    {
        return new Unit
        {
            TopicId = topicId,
            Name = name,
            Description = description,
            OrderIndex = orderIndex,
            TotalLessons = 0
        };
    }
}
