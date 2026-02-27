using Blog.Application.Categories.Models;
using MediatR;

namespace Blog.Application.Categories.GetAll;

public record GetCategoriesQuery : IRequest<List<CategoryVm>>;
