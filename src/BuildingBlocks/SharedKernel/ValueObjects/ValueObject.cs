namespace SharedKernel.ValueObjects
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object? obj)
        {
            return obj is ValueObject other && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public bool Equals(ValueObject? other)
        {
            return other is not null && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Select(x => x?.GetHashCode() ?? 0)
                .Aggregate((x, y) => x ^ y);
        }

        public static bool operator ==(ValueObject? left, ValueObject? right)
        {
            if (left is null && right is null) { return true; }
            if (left is null || right is null) { return false; }
            return left.Equals(right);
        }

        public static bool operator !=(ValueObject? left, ValueObject? right)
        {
            return !(left == right);
        }
    }
}
