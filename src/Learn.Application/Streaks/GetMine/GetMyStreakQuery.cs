using Learn.Application.Streaks.GetMine.Models;
using MediatR;

namespace Learn.Application.Streaks.GetMine;

public record GetMyStreakQuery : IRequest<StreakVm>;
