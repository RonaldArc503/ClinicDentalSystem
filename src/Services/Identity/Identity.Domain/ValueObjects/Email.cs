using SharedKernel.ValueObjects;

namespace Identity.Domain.ValueObjects
{
    public sealed class Email : ValueObject
    {
        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Email cannot be empty.", nameof(value));
            }

            if (!value.Contains('@') || !value.Contains('.'))
            {
                throw new ArgumentException("Email must contain '@' and a domain.", nameof(value));
            }

            string[] parts = value.Split('@');
            if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            {
                throw new ArgumentException("Email format is invalid.", nameof(value));
            }

            return new Email(value.Trim().ToLowerInvariant());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(Email email)
        {
            ArgumentNullException.ThrowIfNull(email);
            return email.Value;
        }
    }
}
