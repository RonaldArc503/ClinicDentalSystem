namespace SharedKernel.Results
{
    public sealed record Error
    {
        public string Code { get; }
        public string Description { get; }

        public Error(string code, string description)
        {
            Code = code;
            Description = description;
        }

        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");
        public static readonly Error ConditionFailed = new("Error.ConditionFailed", "One or more conditions were not met.");

        public static Error NotFound(string entityName, object id) =>
            new("Error.NotFound", $"The entity '{entityName}' with identifier '{id}' was not found.");

        public static Error ValidationFailed(string detail) =>
            new("Error.ValidationFailed", detail);

        public static Error Conflict(string detail) =>
            new("Error.Conflict", detail);
    }
}
