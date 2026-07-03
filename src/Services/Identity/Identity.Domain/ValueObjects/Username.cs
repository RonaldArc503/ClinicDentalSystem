using SharedKernel.ValueObjects;

namespace Identity.Domain.ValueObjects
{
    public sealed class Username : ValueObject
    {
        public string Value { get; }

        private Username(string value)
        {
            Value = value;
        }

        public static Username Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Username cannot be empty.", nameof(value));

            value = value.Trim();

            if (value.Length < 3)
                throw new ArgumentException("Username must be at least 3 characters long.", nameof(value));

            if (value.Length > 30)
                throw new ArgumentException("Username cannot exceed 30 characters.", nameof(value));

            if (!value.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '.'))
                throw new ArgumentException("Username can only contain letters, digits, underscores, and periods.", nameof(value));

            return new Username(value);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;

        public static implicit operator string(Username username) => username.Value;
    }
}
