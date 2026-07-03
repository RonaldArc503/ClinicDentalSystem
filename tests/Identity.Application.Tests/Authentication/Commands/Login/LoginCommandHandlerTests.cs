using FluentAssertions;
using Identity.Application.Authentication.Commands.Login;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using Moq;

namespace Identity.Application.Tests.Authentication.Commands.Login
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _handler = new LoginCommandHandler(
                _userRepositoryMock.Object,
                _refreshTokenRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _passwordHasherMock.Object,
                _jwtTokenGeneratorMock.Object);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsInvalidOperationException()
        {
            var command = new LoginCommand("nonexistent@test.com", "Password123!");
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync((User?)null);

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid credentials.");
        }

        [Fact]
        public async Task Handle_InactiveUser_ThrowsInvalidOperationException()
        {
            var user = CreateActiveUser();
            user.Deactivate();
            var command = new LoginCommand("test@test.com", "Password123!");

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid credentials.");
        }

        [Fact]
        public async Task Handle_WrongPassword_ThrowsInvalidOperationException()
        {
            var user = CreateActiveUser();
            var command = new LoginCommand("test@test.com", "WrongPassword1!");

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify("WrongPassword1!", user.PasswordHash))
                .Returns(false);

            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Invalid credentials.");
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsLoginResponse()
        {
            var user = CreateActiveUser();
            var command = new LoginCommand("test@test.com", "CorrectPassword1!");

            _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<Email>()))
                .ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify("CorrectPassword1!", user.PasswordHash))
                .Returns(true);
            _jwtTokenGeneratorMock.Setup(x => x.GenerateAccessTokenAsync(user))
                .ReturnsAsync("access-token");
            _jwtTokenGeneratorMock.Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.AccessToken.Should().Be("access-token");
            result.RefreshToken.Should().Be("refresh-token");
            result.TokenType.Should().Be("Bearer");
            result.User.Id.Should().Be(user.Id);
            result.User.Email.Should().Be(user.Email);

            _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Identity.Domain.Entities.RefreshToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private static User CreateActiveUser()
        {
            var user = User.Create(
                Guid.NewGuid(),
                PersonName.Create("Juan", "Perez"),
                Email.Create("test@test.com"),
                Username.Create("juanperez"),
                BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!"));

            return user;
        }
    }
}
