using MediatR;

namespace Identity.Application.Authentication.Commands.Logout
{
    public sealed record LogoutCommand(
        Guid UserId) : IRequest;
}
