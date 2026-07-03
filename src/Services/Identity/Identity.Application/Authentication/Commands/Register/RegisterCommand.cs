using MediatR;

namespace Identity.Application.Authentication.Commands.Register
{
    public sealed record RegisterCommand(
        string FirstName,
        string LastName,
        string Email,
        string Username,
        string Password) : IRequest<RegisterResponse>;
}
