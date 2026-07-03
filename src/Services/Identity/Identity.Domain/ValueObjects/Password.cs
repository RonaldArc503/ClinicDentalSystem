using SharedKernel.ValueObjects;

namespace Identity.Domain.ValueObjects
{
    public sealed class Password : ValueObject
    {
        public string Value { get; }

        private Password(string value)
        {
            Value = value;
        }

        public static Password Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(value));
            }

            if (value.Length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters long.", nameof(value));
            }

            if (value.Length > 128)
            {
                throw new ArgumentException("Password cannot exceed 128 characters.", nameof(value));
            }

            if (!value.Any(char.IsUpper))
            {
                throw new ArgumentException("Password must contain at least one uppercase letter.", nameof(value));
            }

            if (!value.Any(char.IsLower))
            {
                throw new ArgumentException("Password must contain at least one lowercase letter.", nameof(value));
            }

            if (!value.Any(char.IsDigit))
            {
                throw new ArgumentException("Password must contain at least one digit.", nameof(value));
            }

            if (!value.Any(c => !char.IsLetterOrDigit(c)))
            {
                throw new ArgumentException("Password must contain at least one special character.", nameof(value));
            }

            return new Password(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(Password password)
        {
            ArgumentNullException.ThrowIfNull(password);
            return password.Value;
        }
    }
}
