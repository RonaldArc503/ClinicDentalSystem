using SharedKernel.Entities;

namespace Identity.Domain.Entities
{
    public sealed class Permission : BaseEntity<int>
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string Category { get; private set; } = string.Empty;

        private Permission(int id, string name, string category, string? description)
            : base(id)
        {
            Name = name;
            Category = category;
            Description = description;
        }

        private Permission() { }

        public static Permission Create(int id, string name, string category, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Permission name cannot be empty.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Permission category cannot be empty.", nameof(category));
            }

            return new Permission(id, name.Trim(), category.Trim(), description?.Trim());
        }

        public void UpdateDescription(string description)
        {
            Description = description?.Trim();
        }
    }
}
