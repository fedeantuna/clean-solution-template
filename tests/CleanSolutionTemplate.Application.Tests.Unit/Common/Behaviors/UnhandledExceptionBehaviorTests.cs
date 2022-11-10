using CleanSolutionTemplate.Application.Common.Behaviors;
using CleanSolutionTemplate.Application.Tests.Unit.Fakes;
using FluentAssertions;
using MediatR;
using MediatR.Pipeline;
using Moq;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;

namespace CleanSolutionTemplate.Application.Tests.Unit.Common.Behaviors;

public class UnhandledExceptionBehaviorTests : TestBase
{
    private readonly UnhandledExceptionBehavior<IRequest<string>, Exception> _sut;

    public UnhandledExceptionBehaviorTests()
    {
        var pipelineBehaviors = this.FindService<IEnumerable<IRequestExceptionAction<IRequest<string>, Exception>>>();
        this._sut = (UnhandledExceptionBehavior<IRequest<string>, Exception>)pipelineBehaviors.First(pb =>
            pb.GetType().Name == typeof(UnhandledExceptionBehavior<,>).Name);
    }

    [Fact]
    public async Task Execute_LogsError_WhenRequestHandlerIsNotValid()
    {
        // Arrange
        var requestMock = new Mock<IRequest<string>>();
        var exception = new Exception("test-exception");
        var cancellationToken = default(CancellationToken);

        // Act
        await this._sut.Execute(requestMock.Object, exception, cancellationToken);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(UnhandledExceptionBehavior<IRequest<string>, Exception>.LogMessageTemplate).Once()
            .WithProperty("RequestName").WithValue(typeof(IRequest<>).Name)
            .And.WithProperty("Request").HavingADestructuredObject();
    }

    [Fact]
    public async Task Execute_ProcessUnhandledExceptions()
    {
        // Arrange
        var mediator = this.FindService<IMediator>();

        var request = new FakeFailingRequest();

        // Act
        var act = async () => await mediator.Send(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }
}
