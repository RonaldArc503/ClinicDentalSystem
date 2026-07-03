using SharedKernel.Entities;

namespace Identity.Domain.Entities
{
    public sealed class RefreshToken : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public User? User { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        private RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt)
            : base(id)
        {
            UserId = userId;
            Token = token;
            ExpiresAt = expiresAt;
        }

        private RefreshToken() { }

        public static RefreshToken Create(Guid id, Guid userId, string token, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be empty.", nameof(token));
            }

            if (expiresAt <= DateTime.UtcNow)
            {
                throw new ArgumentException("Expiration date must be in the future.", nameof(expiresAt));
            }

            return new RefreshToken(id, userId, token, expiresAt);
        }

        public void Revoke(string? replacedByToken = null)
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            ReplacedByToken = replacedByToken;
        }
    }
}
