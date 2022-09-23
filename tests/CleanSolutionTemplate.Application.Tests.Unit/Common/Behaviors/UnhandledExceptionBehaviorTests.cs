using System.Text;
using CleanSolutionTemplate.Application.Common.Behaviors;
using CleanSolutionTemplate.Application.Tests.Unit.Fakes;
using FluentAssertions;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Moq;

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
        var fakeLogger = (FakeLogger<IRequest<string>>)this.FindService<ILogger<IRequest<string>>>();

        var requestMock = new Mock<IRequest<string>>();
        var exception = new Exception("test-exception");
        var cancellationToken = default(CancellationToken);

        var stateBuilder = new StringBuilder(UnhandledExceptionBehavior<IRequest<string>, Exception>.LogMessageTemplate);
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
        await this._sut.Execute(requestMock.Object, exception, cancellationToken);

        // Assert
        fakeLogger.Category.Should().Be(typeof(IRequest<>).Name);
        fakeLogger.LogRecords.Should().Equal(expectedLogRecords);
    }

    [Fact]
    public async Task Execute_ProcessUnhandledExceptions()
    {
        // Arrange
        var fakeLogger = (FakeLogger<FakeFailingRequest>)this.FindService<ILogger<FakeFailingRequest>>();
        var mediator = this.FindService<IMediator>();

        var request = new FakeFailingRequest();

        var stateBuilder = new StringBuilder(UnhandledExceptionBehavior<FakeFailingRequest, Exception>.LogMessageTemplate);
        stateBuilder.Replace("{requestName}", nameof(FakeFailingRequest));
        stateBuilder.Replace("{@request}", request.ToString());
        var expectedState = stateBuilder.ToString();
        var expectedLogRecord = new LogRecord()
        {
            LogLevel = (int)LogLevel.Error,
            EventId = 0,
            State = expectedState,
            Exception = request.Exception
        };

        // Act
        var act = async () => await mediator.Send(request);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        fakeLogger.LogRecords.Last().Should().Be(expectedLogRecord);
    }
}
