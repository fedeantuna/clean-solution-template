using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Infrastructure.Common;
using CleanSolutionTemplate.Infrastructure.Persistence.Interceptors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanSolutionTemplate.Infrastructure.Persistence;

internal class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;
    private readonly IPublisher _publisher;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        IPublisher publisher,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options)
    {
        this._publisher = publisher;
        this._auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(this._auditableEntitySaveChangesInterceptor);

    public async Task<int> SaveChangesAsync() =>
        await this.SaveChangesAsync(default);

    [SuppressMessage("ReSharper", "OptionalParameterHierarchyMismatch")]
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        await this._publisher.DispatchDomainEvents(this, cancellationToken);

        return await base.SaveChangesAsync(cancellationToken);
    }
}
