using CleanSolutionTemplate.Application.Common.Behaviors;
using CleanSolutionTemplate.Application.Tests.Unit.Fakes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;

namespace CleanSolutionTemplate.Application.Tests.Unit.Common.Behaviors;

public class LoggingBehaviorTests
{
    private readonly ISender _sender;

    public LoggingBehaviorTests()
    {
        var provider = new ServiceProviderBuilder().Build();

        this._sender = provider.GetRequiredService<ISender>();
    }

    [Fact]
    public async Task LogsInformationAboutTheUserAndTheRequest()
    {
        // Arrange
        var request = new UnvalidatedPassingRequestFake();
        var cancellationToken = default(CancellationToken);

        // Act
        await this._sender.Send(request, cancellationToken);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(LoggingBehavior<object>.LogMessageTemplate).Once()
            .WithProperty("RequestName").WithValue(nameof(UnvalidatedPassingRequestFake))
            .And.WithProperty("UserId").WithValue(Constants.TestUserId)
            .And.WithProperty("Request").HavingADestructuredObject();
    }
}
