using MediatR;

namespace Blog.Application.Categories.Upsert;

public class UpsertCategoryCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
