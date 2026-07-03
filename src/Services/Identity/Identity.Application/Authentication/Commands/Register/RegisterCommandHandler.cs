using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;
using MediatR;

namespace Identity.Application.Authentication.Commands.Register
{
    public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
        }

        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var email = Email.Create(request.Email);
            var username = Username.Create(request.Username);

            if (await _userRepository.ExistsByEmailAsync(email))
                throw new InvalidOperationException($"Email '{request.Email}' is already registered.");

            if (await _userRepository.ExistsByUsernameAsync(username))
                throw new InvalidOperationException($"Username '{request.Username}' is already taken.");

            var personName = PersonName.Create(request.FirstName, request.LastName);
            var passwordHash = _passwordHasher.Hash(request.Password);

            var user = User.Create(Guid.NewGuid(), personName, email, username, passwordHash);

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new RegisterResponse(
                user.Id,
                user.Email,
                user.Username,
                user.CreatedAt);
        }
    }
}
