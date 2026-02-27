namespace Blog.Domain.Entities;

public interface ICreatedEntity
{
    DateTime CreatedDate { get; set; }
    DateTime? ModifiedDate { get; set; }
}
