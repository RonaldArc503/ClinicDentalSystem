using Identity.Application.Authentication.Commands.Login;
using MediatR;

namespace Identity.Application.Authentication.Commands.RefreshToken
{
    public sealed record RefreshTokenCommand(
        string RefreshToken) : IRequest<LoginResponse>;
}
