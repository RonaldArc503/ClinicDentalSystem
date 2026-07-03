using Identity.Application.Interfaces;
using Identity.Domain.ValueObjects;
using MediatR;

namespace Identity.Application.Authentication.Commands.Login
{
    public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            Domain.Entities.User? user = await FindUserAsync(request.EmailOrUsername).ConfigureAwait(false);

            if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid credentials.");
            }

            user.MarkLogin();

            string accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user).ConfigureAwait(false);
            string refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            Domain.Entities.RefreshToken refreshTokenEntity = Domain.Entities.RefreshToken.Create(
                Guid.NewGuid(),
                user.Id,
                refreshToken,
                DateTime.UtcNow.AddDays(7));

            await _refreshTokenRepository.AddAsync(refreshTokenEntity).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new LoginResponse(
                accessToken,
                refreshToken,
                3600,
                "Bearer",
                new UserInfo(user.Id, user.PersonName.FullName, user.Email));
        }

        private async Task<Domain.Entities.User?> FindUserAsync(string emailOrUsername)
        {
            if (emailOrUsername.Contains('@'))
            {
                Email email = Email.Create(emailOrUsername);
                return await _userRepository.GetByEmailAsync(email).ConfigureAwait(false);
            }

            Username username = Username.Create(emailOrUsername);
            return await _userRepository.GetByUsernameAsync(username).ConfigureAwait(false);
        }
    }
}
