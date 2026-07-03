using SharedKernel.Entities;

namespace Identity.Domain.Entities
{
    public sealed class RolePermission : BaseEntity<int>
    {
        public int RoleId { get; private set; }
        public int PermissionId { get; private set; }
        public Role? Role { get; private set; }
        public Permission? Permission { get; private set; }

        private RolePermission(int id, int roleId, int permissionId)
            : base(id)
        {
            RoleId = roleId;
            PermissionId = permissionId;
        }

        private RolePermission() { }

        public static RolePermission Create(int id, int roleId, int permissionId)
        {
            return new RolePermission(id, roleId, permissionId);
        }
    }
}
