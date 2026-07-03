namespace Identity.Application.Authentication.Commands.Register
{
    public sealed record RegisterResponse(
        Guid UserId,
        string Email,
        string Username,
        DateTime CreatedAt);
}
