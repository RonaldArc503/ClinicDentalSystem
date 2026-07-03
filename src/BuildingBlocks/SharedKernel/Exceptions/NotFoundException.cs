namespace SharedKernel.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, object id)
            : base($"The entity '{entityName}' with identifier '{id}' was not found.")
        {
        }
    }
}
