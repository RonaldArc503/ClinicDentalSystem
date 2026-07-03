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
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (existingToken is null || !existingToken.IsActive)
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            var user = await _userRepository.GetByIdAsync(existingToken.UserId);

            if (user is null || !user.IsActive)
            {
                throw new InvalidOperationException("Invalid refresh token.");
            }

            var newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
            existingToken.Revoke(newRefreshTokenString);
            await _refreshTokenRepository.UpdateAsync(existingToken);

            var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);

            var newRefreshTokenEntity = Domain.Entities.RefreshToken.Create(
                Guid.NewGuid(),
                user.Id,
                newRefreshTokenString,
                DateTime.UtcNow.AddDays(7));

            await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new LoginResponse(
                accessToken,
                newRefreshTokenString,
                3600,
                "Bearer",
                new UserInfo(user.Id, user.PersonName.FullName, user.Email));
        }
    }
}
