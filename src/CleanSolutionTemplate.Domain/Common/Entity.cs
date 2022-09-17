using System.ComponentModel.DataAnnotations.Schema;

namespace CleanSolutionTemplate.Domain.Common;

public abstract class Entity
{
    readonly List<DomainEvent> _domainEvents = new();

    protected Entity(Guid id) => this.Id = id;

    public Guid Id { get; private set; }

    [NotMapped] public IReadOnlyList<DomainEvent> DomainEvents => this._domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent domainEvent) => this._domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(DomainEvent domainEvent) => this._domainEvents.Remove(domainEvent);

    public void RemoveRange(int index) => this._domainEvents.RemoveRange(0, index);
}
