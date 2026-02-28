using Learn.Application.Profile.GetProfile.Models;
using MediatR;

namespace Learn.Application.Profile.GetProfile;

public record GetProfileQuery : IRequest<UserProfileVm>;
