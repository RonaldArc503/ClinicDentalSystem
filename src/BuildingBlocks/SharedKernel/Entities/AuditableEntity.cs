using SharedKernel.Interfaces;

namespace SharedKernel.Entities
{
    public abstract class AuditableEntity<TId> : BaseEntity<TId>, IAuditable
        where TId : notnull
    {
        public DateTime CreatedAt { get; protected set; }
        public string CreatedBy { get; protected set; } = string.Empty;
        public DateTime? UpdatedAt { get; protected set; }
        public string? UpdatedBy { get; protected set; }

        protected AuditableEntity(TId id) : base(id) { }

        protected AuditableEntity() { }

        public void SetCreationInfo(string createdBy, DateTime createdAt)
        {
            CreatedBy = createdBy;
            CreatedAt = createdAt;
        }

        public void SetModificationInfo(string updatedBy, DateTime updatedAt)
        {
            UpdatedBy = updatedBy;
            UpdatedAt = updatedAt;
        }
    }
}
