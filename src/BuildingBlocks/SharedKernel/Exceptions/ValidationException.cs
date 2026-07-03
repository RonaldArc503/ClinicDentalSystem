namespace SharedKernel.Exceptions
{
    public class ValidationException : DomainException
    {
        public IReadOnlyCollection<string> Errors { get; }

        public ValidationException()
        {
            Errors = [];
        }

        public ValidationException(string message)
            : base(message)
        {
            Errors = [];
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Errors = [];
        }

        public ValidationException(IReadOnlyCollection<string> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }
}
