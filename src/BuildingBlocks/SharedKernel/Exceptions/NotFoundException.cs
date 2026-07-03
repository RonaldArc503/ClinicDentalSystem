namespace SharedKernel.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException()
        {
        }

        public NotFoundException(string message)
            : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotFoundException(string entityName, object id)
            : base($"The entity '{entityName}' with identifier '{id}' was not found.")
        {
        }
    }
}
