namespace CleanSolutionTemplate.Domain.Common;

public abstract class AuditableEntity : Entity
{
    protected AuditableEntity(Guid id)
        : base(id)
    {
    }

    public string? CreatedBy { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTimeOffset? LastModifiedAt { get; set; }
}
