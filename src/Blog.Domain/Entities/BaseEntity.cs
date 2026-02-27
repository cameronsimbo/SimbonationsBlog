namespace Blog.Domain.Entities;

public abstract class BaseEntity<T> : IEquatable<T> where T : BaseEntity<T>
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public bool Equals(T? other)
    {
        return other is not null && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is T entity && Equals(entity);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
