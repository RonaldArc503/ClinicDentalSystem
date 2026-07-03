using MediatR;

namespace Identity.Application.Authentication.Commands.Login
{
    public sealed record LoginCommand(
        string EmailOrUsername,
        string Password) : IRequest<LoginResponse>;
}
