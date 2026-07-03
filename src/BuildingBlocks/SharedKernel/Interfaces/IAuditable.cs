namespace SharedKernel.Interfaces
{
    public interface IAuditable
    {
        DateTime CreatedAt { get; }
        string CreatedBy { get; }
        DateTime? UpdatedAt { get; }
        string? UpdatedBy { get; }

        void SetCreationInfo(string createdBy, DateTime createdAt);
        void SetModificationInfo(string updatedBy, DateTime updatedAt);
    }
}
