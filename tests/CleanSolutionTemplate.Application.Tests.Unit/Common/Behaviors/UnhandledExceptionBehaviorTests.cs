using System.Text;
using CleanSolutionTemplate.Application.Common.Behaviors;
using CleanSolutionTemplate.Application.Tests.Unit.Fakes;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace CleanSolutionTemplate.Application.Tests.Unit.Common.Behaviors;

public class UnhandledExceptionBehaviorTests : TestBase
{
    private readonly FakeLogger<IRequest<string>> _fakeLogger;

    private readonly UnhandledExceptionBehavior<IRequest<string>, string> _sut;

    public UnhandledExceptionBehaviorTests()
    {
        this._fakeLogger = (FakeLogger<IRequest<string>>)this.FindService<ILogger<IRequest<string>>>();

        var pipelineBehaviors = this.FindService<IEnumerable<IPipelineBehavior<IRequest<string>, string>>>();
        this._sut = (UnhandledExceptionBehavior<IRequest<string>, string>)pipelineBehaviors.First(pb =>
            pb.GetType().Name == typeof(UnhandledExceptionBehavior<,>).Name);
    }

    [Fact]
    public async Task Handle_ReturnsRequestHandlerResult()
    {
        // Arrange
        var requestMock = new Mock<IRequest<string>>();
        var cancellationToken = default(CancellationToken);
        const string handlerResponse = "test-handler-response";

        Task<string> Handler()
        {
            return Task.FromResult(handlerResponse);
        }

        // Act
        var result = await this._sut.Handle(requestMock.Object, cancellationToken, Handler);

        // Assert
        result.Should().Be(handlerResponse);
    }

    [Fact]
    public async Task Handle_LogsErrorAndThrowsException_WhenRequestHandlerIsNotValid()
    {
        // Arrange
        var exception = new Exception("test-exception");

        var requestMock = new Mock<IRequest<string>>();
        var cancellationToken = default(CancellationToken);
        Task<string> Handler()
        {
            return Task.FromException<string>(exception);
        }

        var stateBuilder = new StringBuilder(UnhandledExceptionBehavior<IRequest<object>, object>.LogMessageTemplate);
        stateBuilder.Replace("{requestName}", typeof(IRequest<>).Name);
        stateBuilder.Replace("{@request}", requestMock.Object.ToString());
        var expectedState = stateBuilder.ToString();
        var expectedLogRecords = new List<LogRecord>
        {
            new()
            {
                LogLevel = (int)LogLevel.Error,
                EventId = 0,
                State = expectedState,
                Exception = exception
            }
        };

        // Act
        Func<Task> act = () => this._sut.Handle(requestMock.Object, cancellationToken, Handler);

        // Assert
        await act.Should().ThrowAsync<Exception>(exception.Message);
        this._fakeLogger.Category.Should().Be(typeof(IRequest<>).Name);
        this._fakeLogger.LogRecords.Should().Equal(expectedLogRecords);
    }
}