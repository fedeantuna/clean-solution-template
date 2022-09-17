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

        foreach (var entity in entities)
        {
            var index = 0;
            try
            {
                foreach (var domainEvent in entity.DomainEvents)
                {
                    await publisher.Publish(domainEvent, cancellationToken);
                    index++;
                }
            }
            finally
            {
                // In case of a failure, we only remove the events already published.
                entity.RemoveRange(index);
            }
        }
    }
}
