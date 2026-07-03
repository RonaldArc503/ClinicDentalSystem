using SharedKernel.Entities;

namespace Identity.Domain.Entities
{
    public sealed class UserRole : BaseEntity<int>
    {
        public Guid UserId { get; private set; }
        public int RoleId { get; private set; }
        public User? User { get; private set; }
        public Role? Role { get; private set; }

        private UserRole(int id, Guid userId, int roleId)
            : base(id)
        {
            UserId = userId;
            RoleId = roleId;
        }

        private UserRole() { }

        public static UserRole Create(int id, Guid userId, int roleId)
        {
            return new UserRole(id, userId, roleId);
        }
    }
}
