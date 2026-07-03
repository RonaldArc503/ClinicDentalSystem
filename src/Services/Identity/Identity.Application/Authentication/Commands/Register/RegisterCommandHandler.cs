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
            ArgumentNullException.ThrowIfNull(request);

            Email email = Email.Create(request.Email);
            Username username = Username.Create(request.Username);

            if (await _userRepository.ExistsByEmailAsync(email).ConfigureAwait(false))
            {
                throw new InvalidOperationException($"Email '{request.Email}' is already registered.");
            }

            if (await _userRepository.ExistsByUsernameAsync(username).ConfigureAwait(false))
            {
                throw new InvalidOperationException($"Username '{request.Username}' is already taken.");
            }

            PersonName personName = PersonName.Create(request.FirstName, request.LastName);
            string passwordHash = _passwordHasher.Hash(request.Password);

            User user = User.Create(Guid.NewGuid(), personName, email, username, passwordHash);

            await _userRepository.AddAsync(user).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new RegisterResponse(
                user.Id,
                user.Email,
                user.Username,
                user.CreatedAt);
        }
    }
}
