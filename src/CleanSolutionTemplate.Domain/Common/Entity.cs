using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace CleanSolutionTemplate.Domain.Common;

public abstract class Entity
{
    private readonly List<DomainEvent> _domainEvents = new();

    protected Entity(Guid id) =>
        this.Id = id;

    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
    public Guid Id { get; init; }

    [NotMapped]
    [SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
    public IReadOnlyCollection<DomainEvent> DomainEvents =>
        this._domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent domainEvent) =>
        this._domainEvents.Add(domainEvent);

    public void RemoveDomainEvent(DomainEvent domainEvent) =>
        this._domainEvents.Remove(domainEvent);

    public void ClearDomainEvents() =>
        this._domainEvents.Clear();
}
