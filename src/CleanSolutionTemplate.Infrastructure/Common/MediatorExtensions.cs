using CleanSolutionTemplate.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanSolutionTemplate.Infrastructure.Common;

public static class MediatorExtensions
{
    public static async Task DispatchDomainEvents(this IPublisher publisher,
        DbContext context,
        CancellationToken cancellationToken = default)
    {
        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, cancellationToken);
    }
}
