using CleanSolutionTemplate.Application.Common.Behaviors;
using CleanSolutionTemplate.Application.Tests.Unit.Fakes;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;

namespace CleanSolutionTemplate.Application.Tests.Unit.Common.Behaviors;

public class UnhandledExceptionBehaviorTests
{
    private readonly ISender _sender;

    public UnhandledExceptionBehaviorTests()
    {
        var provider = new ServiceProviderBuilder().Build();

        this._sender = provider.GetRequiredService<ISender>();
    }

    [Fact]
    public async Task LogsErrorWhenRequestFails()
    {
        // Arrange
        var request = new FailingRequestFake();
        var cancellationToken = default(CancellationToken);

        // Act
        var act = () => this._sender.Send(request, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        InMemorySink.Instance
            .Should()
            .HaveMessage(UnhandledExceptionBehavior<IRequest, Exception>.LogMessageTemplate).Once()
            .WithProperty("RequestName").WithValue(nameof(FailingRequestFake))
            .And.WithProperty("Request").HavingADestructuredObject();
    }
}
