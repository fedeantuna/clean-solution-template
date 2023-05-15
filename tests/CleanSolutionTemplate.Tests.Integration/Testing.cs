using CleanSolutionTemplate.Infrastructure.Persistence;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanSolutionTemplate.Tests.Integration;

public static class Testing
{
    private const int TestDatabaseContainerPort = 5433;

    private static readonly WebApplicationFactory<Program> TestWebApplicationFactory = new TestWebApplicationFactory();

    private static IContainer _testDatabaseContainer = CreateTestDatabaseContainer();

    public static Task StartTestDatabaseContainer() =>
        _testDatabaseContainer.StartAsync();

    public static Task StopTestDatabaseContainer() =>
        _testDatabaseContainer.StopAsync();

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = TestWebApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = TestWebApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = TestWebApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = TestWebApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = TestWebApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    private static IContainer CreateTestDatabaseContainer()
    {
        const string testDatabaseImage = "postgres:15.3-alpine3.18";
        const string testDatabasePassword = "password";

        return _testDatabaseContainer = new ContainerBuilder()
            .WithImage(testDatabaseImage)
            .WithPortBinding(TestDatabaseContainerPort, 5432)
            .WithEnvironment(new Dictionary<string, string>
            {
                { "POSTGRES_PASSWORD", testDatabasePassword },
            })
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }
}
