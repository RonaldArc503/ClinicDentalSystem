using SharedKernel.Entities;

namespace Identity.Domain.Entities
{
    public sealed class Role : BaseEntity<int>
    {
        private readonly List<RolePermission> _rolePermissions = [];

        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

        private Role(int id, string name, string? description)
            : base(id)
        {
            Name = name;
            Description = description;
        }

        private Role() { }

        public static Role Create(int id, string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Role name cannot be empty.", nameof(name));
            }

            return new Role(id, name.Trim(), description?.Trim());
        }

        public void UpdateDescription(string? description)
        {
            Description = description?.Trim();
        }

        public void AddPermission(Permission permission)
        {
            ArgumentNullException.ThrowIfNull(permission);

            if (_rolePermissions.Any(rp => rp.PermissionId == permission.Id))
            {
                return;
            }

            _rolePermissions.Add(RolePermission.Create(default, Id, permission.Id));
        }

        public void RemovePermission(int permissionId)
        {
            RolePermission? rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);

            if (rolePermission is not null)
            {
                _rolePermissions.Remove(rolePermission);
            }
        }
    }
}
