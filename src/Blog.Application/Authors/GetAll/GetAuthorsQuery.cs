using Blog.Application.Authors.Models;
using MediatR;

namespace Blog.Application.Authors.GetAll;

public record GetAuthorsQuery : IRequest<List<AuthorVm>>;
