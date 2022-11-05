namespace CleanSolutionTemplate.Application.Common.Persistence;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
