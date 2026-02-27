using Blog.Application.Authors.Models;
using MediatR;

namespace Blog.Application.Authors.Get;

public record GetAuthorQuery : IRequest<AuthorVm>
{
    public Guid Id { get; init; }
}
