namespace Identity.Application.Authentication.Commands.Login
{
    public sealed record LoginResponse(
        string AccessToken,
        string RefreshToken,
        int ExpiresIn,
        string TokenType,
        UserInfo User);

    public sealed record UserInfo(
        Guid Id,
        string Name,
        string Email);
}
