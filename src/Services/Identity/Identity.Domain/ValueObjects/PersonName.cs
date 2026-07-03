using SharedKernel.ValueObjects;

namespace Identity.Domain.ValueObjects
{
    public sealed class PersonName : ValueObject
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }

        private PersonName()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
        }

        private PersonName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public static PersonName Create(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

            firstName = firstName.Trim();
            lastName = lastName.Trim();

            if (firstName.Length > 100)
                throw new ArgumentException("First name cannot exceed 100 characters.", nameof(firstName));

            if (lastName.Length > 100)
                throw new ArgumentException("Last name cannot exceed 100 characters.", nameof(lastName));

            return new PersonName(firstName, lastName);
        }

        public string FullName => $"{FirstName} {LastName}";

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
        }

        public override string ToString() => FullName;
    }
}
