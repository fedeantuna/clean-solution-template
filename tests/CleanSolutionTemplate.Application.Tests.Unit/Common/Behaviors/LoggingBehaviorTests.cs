using CleanSolutionTemplate.Application.Common.Behaviors;
using MediatR;
using MediatR.Pipeline;
using Moq;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;

namespace CleanSolutionTemplate.Application.Tests.Unit.Common.Behaviors;

public class LoggingBehaviorTests : TestBase
{
    private readonly LoggingBehavior<IBaseRequest> _sut;

    public LoggingBehaviorTests()
    {
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

        // Act
        await this._sut.Process(requestMock.Object, cancellationToken);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(LoggingBehavior<object>.LogMessageTemplate).Once()
            .WithProperty("RequestName").WithValue(nameof(IBaseRequest))
            .And.WithProperty("UserId").WithValue(TestUserId)
            .And.WithProperty("UserEmail").WithValue(TestUserEmail)
            .And.WithProperty("Request").HavingADestructuredObject();
    }
}
