namespace Learn.Domain.Entities;

public abstract class CreatedEntity<T> : BaseEntity<T>, ICreatedEntity where T : CreatedEntity<T>
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
}
