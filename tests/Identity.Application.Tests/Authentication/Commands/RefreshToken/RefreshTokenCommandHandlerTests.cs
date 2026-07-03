using FluentAssertions;
using Identity.Application.Authentication.Commands.Login;
using Identity.Application.Authentication.Commands.RefreshToken;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Moq;

namespace Identity.Application.Tests.Authentication.Commands.RefreshToken
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
        private readonly RefreshTokenCommandHandler _handler;

        public RefreshTokenCommandHandlerTests()
        {
            _handler = new RefreshTokenCommandHandler(
                _refreshTokenRepositoryMock.Object,
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _jwtTokenGeneratorMock.Object);
        }

        [Fact]
        public async Task Handle_TokenNotFound_ThrowsInvalidOperationException()
        {
            var command = new RefreshTokenCommand("nonexistent-token");
            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("nonexistent-token"))
                .ReturnsAsync((Domain.Entities.RefreshToken?)null);

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid refresh token.");
        }

        [Fact]
        public async Task Handle_ExpiredToken_ThrowsInvalidOperationException()
        {
            var expiredToken = Domain.Entities.RefreshToken.Create(
                Guid.NewGuid(), Guid.NewGuid(), "expired-token", DateTime.UtcNow.AddDays(1));

            typeof(Domain.Entities.RefreshToken)
                .GetProperty(nameof(Domain.Entities.RefreshToken.ExpiresAt))!
                .SetValue(expiredToken, DateTime.UtcNow.AddDays(-1));

            var command = new RefreshTokenCommand("expired-token");

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("expired-token"))
                .ReturnsAsync(expiredToken);

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid refresh token.");
        }

        [Fact]
        public async Task Handle_RevokedToken_ThrowsInvalidOperationException()
        {
            var revokedToken = Domain.Entities.RefreshToken.Create(
                Guid.NewGuid(), Guid.NewGuid(), "revoked-token", DateTime.UtcNow.AddDays(7));
            revokedToken.Revoke();
            var command = new RefreshTokenCommand("revoked-token");

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("revoked-token"))
                .ReturnsAsync(revokedToken);

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid refresh token.");
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var activeToken = Domain.Entities.RefreshToken.Create(
                Guid.NewGuid(), userId, "valid-token", DateTime.UtcNow.AddDays(7));
            var command = new RefreshTokenCommand("valid-token");

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("valid-token"))
                .ReturnsAsync(activeToken);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid refresh token.");
        }

        [Fact]
        public async Task Handle_InactiveUser_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var user = CreateActiveUser(userId);
            user.Deactivate();
            var activeToken = Domain.Entities.RefreshToken.Create(
                Guid.NewGuid(), userId, "valid-token", DateTime.UtcNow.AddDays(7));
            var command = new RefreshTokenCommand("valid-token");

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("valid-token"))
                .ReturnsAsync(activeToken);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid refresh token.");
        }

        [Fact]
        public async Task Handle_ValidToken_ReturnsLoginResponse()
        {
            var userId = Guid.NewGuid();
            var user = CreateActiveUser(userId);
            var activeToken = Domain.Entities.RefreshToken.Create(
                Guid.NewGuid(), userId, "valid-token", DateTime.UtcNow.AddDays(7));
            var command = new RefreshTokenCommand("valid-token");

            _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync("valid-token"))
                .ReturnsAsync(activeToken);
            _userRepositoryMock.Setup(x => x.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _jwtTokenGeneratorMock.Setup(x => x.GenerateAccessTokenAsync(user))
                .ReturnsAsync("new-access-token");
            _jwtTokenGeneratorMock.Setup(x => x.GenerateRefreshToken())
                .Returns("new-refresh-token");

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.AccessToken.Should().Be("new-access-token");
            result.RefreshToken.Should().Be("new-refresh-token");
            result.TokenType.Should().Be("Bearer");
            result.User.Id.Should().Be(user.Id);
            result.User.Email.Should().Be(user.Email);

            activeToken.IsRevoked.Should().BeTrue();
            activeToken.ReplacedByToken.Should().Be("new-refresh-token");

            _refreshTokenRepositoryMock.Verify(x => x.UpdateAsync(activeToken), Times.Once);
            _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.RefreshToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private static User CreateActiveUser(Guid? id = null)
        {
            var user = User.Create(
                id ?? Guid.NewGuid(),
                PersonName.Create("Juan", "Perez"),
                Email.Create("test@test.com"),
                Username.Create("juanperez"),
                BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!"));

            return user;
        }
    }
}
