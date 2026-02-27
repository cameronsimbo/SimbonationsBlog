using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Application.Tags.Upsert;

public class UpsertTagCommandHandler : IRequestHandler<UpsertTagCommand, Guid>
{
    private readonly IBlogDbContext _db;

    public UpsertTagCommandHandler(IBlogDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(UpsertTagCommand request, CancellationToken cancellationToken)
    {
        Tag entity;

        if (request.Id.HasValue)
        {
            entity = await _db.Tags
                .FirstOrDefaultAsync(t => t.Id == request.Id.Value, cancellationToken)
                ?? throw new Common.Exceptions.NotFoundException(nameof(Tag), request.Id.Value);

            entity.Name = request.Name;
        }
        else
        {
            entity = Tag.Create(request.Name);
            _db.Tags.Add(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
