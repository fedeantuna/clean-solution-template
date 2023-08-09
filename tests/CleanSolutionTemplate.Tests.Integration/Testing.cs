using System.Diagnostics.CodeAnalysis;
using CleanSolutionTemplate.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanSolutionTemplate.Tests.Integration;

[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public static class Testing
{
    private static readonly IServiceProvider Provider = new ServiceProviderBuilder().Build();

    public static readonly string TestUserId = "test-user-id";
    public static readonly string TestUserEmail = "test-user-email";

    public static DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = Provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = Provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = Provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = Provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = Provider.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }
}
