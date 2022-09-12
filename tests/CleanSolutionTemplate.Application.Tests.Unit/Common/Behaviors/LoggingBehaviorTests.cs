using System.Text;
using CleanSolutionTemplate.Application.Common.Behaviors;
using CleanSolutionTemplate.Application.Tests.Unit.Fakes;
using FluentAssertions;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Moq;

namespace CleanSolutionTemplate.Application.Tests.Unit.Common.Behaviors;

public class LoggingBehaviorTests : TestBase
{
    private readonly FakeLogger<IBaseRequest> _fakeLogger;

    private readonly LoggingBehavior<IBaseRequest> _sut;

    public LoggingBehaviorTests()
    {
        this._fakeLogger = (FakeLogger<IBaseRequest>)this.FindService<ILogger<IBaseRequest>>();

        var preProcessors = this.FindService<IEnumerable<IRequestPreProcessor<IBaseRequest>>>();
        this._sut = (LoggingBehavior<IBaseRequest>)preProcessors.First(pp =>
            pp.GetType().Name == typeof(LoggingBehavior<>).Name);
    }

    [Fact]
    public async Task Process_LogsInformationAboutTheUserAndTheRequest()
    {
        // Arrange
        var requestMock = new Mock<IBaseRequest>();
        var cancellationToken = default(CancellationToken);

        var stateBuilder = new StringBuilder(LoggingBehavior<object>.LogMessageTemplate);
        stateBuilder.Replace("{requestName}", nameof(IBaseRequest));
        stateBuilder.Replace("{userId}", TestUserId);
        stateBuilder.Replace("{userEmail}", TestUserEmail);
        stateBuilder.Replace("{@request}", requestMock.Object.ToString());
        var expectedState = stateBuilder.ToString();
        var expectedLogRecords = new List<LogRecord>
        {
            new()
            {
                LogLevel = (int)LogLevel.Information,
                EventId = 0,
                State = expectedState
            }
        };

        // Act
        await this._sut.Process(requestMock.Object, cancellationToken);

        // Assert
        this._fakeLogger.Category.Should().Be(nameof(IBaseRequest));
        this._fakeLogger.LogRecords.Should().Equal(expectedLogRecords);
    }
}
