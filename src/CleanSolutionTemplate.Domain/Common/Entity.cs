using System.ComponentModel.DataAnnotations.Schema;

namespace CleanSolutionTemplate.Domain.Common;

public abstract class Entity
{
    private readonly List<DomainEvent> _domainEvents = new();

    protected Entity(Guid id)
    {
        this.Id = id;
    }

    public Guid Id { get; private set; }

    [NotMapped] public IEnumerable<DomainEvent> DomainEvents => this._domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        this._domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        this._domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        this._domainEvents.Clear();
    }
}