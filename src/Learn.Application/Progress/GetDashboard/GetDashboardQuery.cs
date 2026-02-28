using Learn.Application.Progress.GetDashboard.Models;
using MediatR;

namespace Learn.Application.Progress.GetDashboard;

public record GetDashboardQuery : IRequest<DashboardVm>;
