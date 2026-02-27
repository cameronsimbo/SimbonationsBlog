using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Categories.Upsert;

public class UpsertCategoryCommandHandler : IRequestHandler<UpsertCategoryCommand, Guid>
{
    private readonly IBlogDbContext _db;

    public UpsertCategoryCommandHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(UpsertCategoryCommand request, CancellationToken cancellationToken)
    {
        Category entity;

        if (request.Id.HasValue)
        {
            entity = await _db.Categories
                .FirstOrDefaultAsync(c => c.Id == request.Id.Value, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(nameof(Category), request.Id.Value);

            entity.Name = request.Name;
            entity.Description = request.Description;
        }
        else
        {
            entity = Category.Create(request.Name, request.Description);
            _db.Categories.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
