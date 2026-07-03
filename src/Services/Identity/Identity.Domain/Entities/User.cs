using Identity.Domain.ValueObjects;
using SharedKernel.Entities;

namespace Identity.Domain.Entities
{
    public sealed class User : AuditableEntity<Guid>
    {
        private readonly List<UserRole> _userRoles = [];

        public PersonName PersonName { get; private set; } = default!;
        public Email Email { get; private set; } = default!;
        public Username Username { get; private set; } = default!;
        public string PasswordHash { get; private set; } = string.Empty;
        public bool IsActive { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        private User(
            Guid id,
            PersonName personName,
            Email email,
            Username username,
            string passwordHash)
            : base(id)
        {
            PersonName = personName;
            Email = email;
            Username = username;
            PasswordHash = passwordHash;
            IsActive = true;
        }

        private User() { }

        public static User Create(
            Guid id,
            PersonName personName,
            Email email,
            Username username,
            string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
            }

            return new User(id, personName, email, username, passwordHash);
        }

        public void UpdatePersonName(PersonName personName)
        {
            PersonName = personName;
        }

        public void UpdateEmail(Email email)
        {
            Email = email;
        }

        public void UpdateUsername(Username username)
        {
            Username = username;
        }

        public void UpdatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
            }

            PasswordHash = passwordHash;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void MarkLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        public void AssignRole(Role role)
        {
            ArgumentNullException.ThrowIfNull(role);

            if (_userRoles.Any(ur => ur.RoleId == role.Id))
            {
                return;
            }

            _userRoles.Add(UserRole.Create(default, Id, role.Id));
        }

        public void RemoveRole(int roleId)
        {
            UserRole? userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);

            if (userRole is not null)
            {
                _userRoles.Remove(userRole);
            }
        }

        public bool HasRole(string roleName)
        {
            return _userRoles.Any(ur =>
                ur.Role is not null &&
                ur.Role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
