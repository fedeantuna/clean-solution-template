using CleanSolutionTemplate.Application.Common.Services;
using CleanSolutionTemplate.Application.Common.Wrappers;
using CleanSolutionTemplate.Domain.Common;
using CleanSolutionTemplate.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CleanSolutionTemplate.Infrastructure.Persistence.Interceptors;

internal class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeOffsetWrapper _dateTimeOffsetWrapper;
    private readonly IUserService _userService;

    public AuditableEntitySaveChangesInterceptor(
        IUserService userService,
        IDateTimeOffsetWrapper dateTimeOffsetWrapper)
    {
        this._userService = userService;
        this._dateTimeOffsetWrapper = dateTimeOffsetWrapper;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        this.UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        this.UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = this._userService.GetCurrentUserId();
                entry.Entity.CreatedAt = this._dateTimeOffsetWrapper.UtcNow;
            }

            if (entry.State != EntityState.Added
                && entry.State != EntityState.Modified
                && !entry.HasChangedOwnedEntities())
                continue;

            entry.Entity.LastModifiedBy = this._userService.GetCurrentUserId();
            entry.Entity.LastModifiedAt = this._dateTimeOffsetWrapper.UtcNow;
        }
    }
}