using Identity.Application.Authentication.Commands.Login;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using MediatR;

namespace Identity.Application.Authentication.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public RefreshTokenCommandHandler(
            IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            Domain.Entities.RefreshToken? existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken).ConfigureAwait(false);

            if (existingToken is null || !existingToken.IsActive)
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            Domain.Entities.User? user = await _userRepository.GetByIdAsync(existingToken.UserId).ConfigureAwait(false);

            if (user is null || !user.IsActive)
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            string newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
            existingToken.Revoke(newRefreshTokenString);
            await _refreshTokenRepository.UpdateAsync(existingToken).ConfigureAwait(false);

            string accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user).ConfigureAwait(false);

            Domain.Entities.RefreshToken newRefreshTokenEntity = Domain.Entities.RefreshToken.Create(
                Guid.NewGuid(),
                user.Id,
                newRefreshTokenString,
                DateTime.UtcNow.AddDays(7));

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new LoginResponse(
                accessToken,
                newRefreshTokenString,
                3600,
                "Bearer",
                new UserInfo(user.Id, user.PersonName.FullName, user.Email));
        }
    }
}
