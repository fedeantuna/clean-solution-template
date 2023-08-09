using CleanSolutionTemplate.Api.SerilogPolicies;
using CleanSolutionTemplate.Api.Tests.Unit.Fakes;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;

namespace CleanSolutionTemplate.Api.Tests.Unit.SerilogPolicies;

public class SensitiveInformationDestructuringPolicyTests
{
    private const string LogMessageTemplate = "Destructuring {@Object}";

    private readonly ModelFake _modelFake;

    private readonly ILogger _logger;

    public SensitiveInformationDestructuringPolicyTests()
    {
        var provider = new ServiceProviderBuilder().Build();

        this._logger = provider.GetRequiredService<ILogger<SensitiveInformationDestructuringPolicyTests>>();

        this._modelFake = CreateFakeModel();
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Logger_ShouldLogCorrespondingLogLevel(LogLevel logLevel)
    {
        // Act
        this.LogFakeModel(logLevel);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage()
            .WithLevel((LogEventLevel)logLevel);
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Logger_ShouldLogSensitiveInformationReplacedWithMask(LogLevel logLevel)
    {
        // Act
        this.LogFakeModel(logLevel);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(LogMessageTemplate).Once()
            .WithDeconstructedFakeModel(nameof(ModelFake.SomeSensitiveStrings))
            .And.WithDeconstructedFakeModel(nameof(ModelFake.SomeSensitiveString))
            .And.WithDeconstructedFakeModel(nameof(ModelFake.SomeSensitiveNumber))
            .And.WithDeconstructedFakeModel(nameof(ModelFake.SomeSensitiveDictionary))
            .And.WithDeconstructedFakeModel(nameof(ModelFake.SomeSensitiveFakeInnerModelRecord));
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Logger_ShouldLogNonSensitiveInformationAsIs_ForNonInnerModelProperties(LogLevel logLevel)
    {
        // Act
        this.LogFakeModel(logLevel);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(LogMessageTemplate).Once()
            .WithDeconstructedFakeModel(nameof(ModelFake.SomeNumbers),
                $"[{string.Join(", ", this._modelFake.SomeNumbers)}]")
            .And.WithDeconstructedFakeModel(nameof(ModelFake.SomeString), this._modelFake.SomeString)
            .And.WithDeconstructedFakeModel(nameof(ModelFake.SomeNumber), this._modelFake.SomeNumber)
            .And.WithDeconstructedFakeModel(nameof(ModelFake.SomeDictionary),
                $"[{string.Join(", ", this._modelFake.SomeDictionary.Select(d => $"({d.Key}: \"{d.Value}\")"))}]");
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Logger_ShouldLogSensitiveInformationReplacedWithMask_ForInnerModelProperties(LogLevel logLevel)
    {
        // Act
        this.LogFakeModel(logLevel);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(LogMessageTemplate).Once()
            .WithDeconstructedFakeInnerModel(nameof(ModelFake.SomeFakeInnerModelRecord),
                nameof(InnerModelRecordFake.SomeSensitiveNumbers))
            .And.WithDeconstructedFakeInnerModel(nameof(ModelFake.SomeFakeInnerModelRecord),
                nameof(InnerModelRecordFake.SomeSensitiveString))
            .And.WithDeconstructedFakeInnerModel(nameof(ModelFake.SomeFakeInnerModelRecord),
                nameof(InnerModelRecordFake.SomeSensitiveNumber))
            .And.WithDeconstructedFakeInnerModel(nameof(ModelFake.SomeFakeInnerModelRecord),
                nameof(InnerModelRecordFake.SomeSensitiveDictionary));
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Logger_ShouldLogNonSensitiveInformationAsIs_ForInnerModelProperties(LogLevel logLevel)
    {
        // Act
        this.LogFakeModel(logLevel);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(LogMessageTemplate).Once()
            .WithDeconstructedFakeInnerModel(nameof(ModelFake.SomeFakeInnerModelRecord),
                nameof(InnerModelRecordFake.SomeStrings),
                $"[{string.Join(", ", this._modelFake.SomeFakeInnerModelRecord.SomeStrings.Select(ss => $"\"{ss}\""))}]")
            .And.WithDeconstructedFakeInnerModel(nameof(ModelFake.SomeFakeInnerModelRecord),
                nameof(InnerModelRecordFake.SomeString), this._modelFake.SomeFakeInnerModelRecord.SomeString)
            .And.WithDeconstructedFakeInnerModel(nameof(ModelFake.SomeFakeInnerModelRecord),
                nameof(InnerModelRecordFake.SomeNumber), this._modelFake.SomeFakeInnerModelRecord.SomeNumber)
            .And.WithDeconstructedFakeInnerModel(nameof(ModelFake.SomeFakeInnerModelRecord),
                nameof(InnerModelRecordFake.SomeDictionary),
                $"[{string.Join(", ", this._modelFake.SomeFakeInnerModelRecord.SomeDictionary.Select(d => $"(\"{d.Key}\": {d.Value})"))}]");
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Logger_ShouldNotDeconstructAnAction(LogLevel logLevel)
    {
        // Arrange
        void TestAction(string _)
        {
            // We don't really care about doing anything here
            // We just care that this is an Action
        }

        // Act
        this.Log(logLevel, (Action<string>)TestAction);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(LogMessageTemplate).Once()
            .WithProperty("Object").WithValue(((Action<string>)TestAction).ToString());
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Logger_ShouldNotDeconstructAFunc(LogLevel logLevel)
    {
        // Arrange
        var testFunc = (string input) => input;

        // Act
        this.Log(logLevel, testFunc);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(LogMessageTemplate).Once()
            .WithProperty("Object").WithValue(testFunc.ToString());
    }

    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Logger_ShouldLogMessageWithExceptionType_WhenReadingAPropertyThrowsException(LogLevel logLevel)
    {
        // Arrange
        var exception = new Exception();
        var fakeUnreadableModel = new UnreadableModelFake(exception);

        // Act
        this.Log(logLevel, fakeUnreadableModel);

        // Assert
        InMemorySink.Instance
            .Should()
            .HaveMessage(LogMessageTemplate).Once()
            .WithProperty("Object").HavingADestructuredObject()
            .WithProperty(nameof(UnreadableModelFake.ThrowingExceptionProperty))
            .WithValue("Property Accessor throws an Exception");
    }

    private static ModelFake CreateFakeModel() =>
        new()
        {
            SomeSensitiveStrings = new List<string> { "sensitive-information-1", "sensitive-information-2" },
            SomeNumbers = new List<int> { 1, 2, 3, 4, 5 },
            SomeSensitiveString = "sensitive-string",
            SomeString = "some-string",
            SomeSensitiveNumber = 42,
            SomeNumber = 0,
            SomeSensitiveDictionary =
            {
                { "firstKey", 0 },
                { "secondKey", 1 }
            },
            SomeDictionary =
            {
                { 0, "firstValue" },
                { 1, "secondValue" }
            },
            SomeSensitiveFakeInnerModelRecord = new InnerModelRecordFake(),
            SomeFakeInnerModelRecord = new InnerModelRecordFake
            {
                SomeSensitiveNumbers = new List<int> { 1, 2, 3 },
                SomeStrings = new List<string> { "first", "second", "third" },
                SomeString = "some-string",
                SomeSensitiveNumber = 42,
                SomeNumber = 107,
                SomeSensitiveDictionary =
                {
                    { 0, "firstValue" },
                    { 1, "secondValue" }
                },
                SomeDictionary =
                {
                    { "firstKey", 0 },
                    { "secondKey", 1 }
                }
            }
        };

    private void LogFakeModel(LogLevel logLevel) => this.Log(logLevel, this._modelFake);

    private void Log(LogLevel logLevel, object @object)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                this._logger.LogTrace(LogMessageTemplate, @object);
                break;
            case LogLevel.Debug:
                this._logger.LogDebug(LogMessageTemplate, @object);
                break;
            case LogLevel.Information:
                this._logger.LogInformation(LogMessageTemplate, @object);
                break;
            case LogLevel.Warning:
                this._logger.LogWarning(LogMessageTemplate, @object);
                break;
            case LogLevel.Error:
                this._logger.LogError(LogMessageTemplate, @object);
                break;
            case LogLevel.Critical:
                this._logger.LogCritical(LogMessageTemplate, @object);
                break;
            case LogLevel.None:
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }
}

public static class SensitiveInformationDestructuringPolicyTestsExtensions
{
    private const string FakeModelProperty = "Object";

    public static AndConstraint<LogEventAssertion> WithDeconstructedFakeModel(this LogEventAssertion logEventAssertion,
        string deconstructedPropertyName) =>
        logEventAssertion
            .WithDeconstructedFakeModelProperty(deconstructedPropertyName)
            .WithValue(SensitiveInformationDestructuringPolicy.Mask);

    public static AndConstraint<LogEventAssertion> WithDeconstructedFakeModel(this LogEventAssertion logEventAssertion,
        string deconstructedPropertyName,
        object? value) =>
        logEventAssertion
            .WithDeconstructedFakeModelProperty(deconstructedPropertyName).WithValue(value);

    public static AndConstraint<LogEventAssertion> WithDeconstructedFakeInnerModel(this LogEventAssertion logEventAssertion,
        string deconstructedPropertyName,
        string innerPropertyName) =>
        logEventAssertion
            .WithDeconstructedFakeModelProperty(deconstructedPropertyName).HavingADestructuredObject()
            .WithProperty(innerPropertyName).WithValue(SensitiveInformationDestructuringPolicy.Mask);

    public static AndConstraint<LogEventAssertion> WithDeconstructedFakeInnerModel(this LogEventAssertion logEventAssertion,
        string deconstructedPropertyName,
        string innerPropertyName,
        object? value) =>
        logEventAssertion
            .WithDeconstructedFakeModelProperty(deconstructedPropertyName).HavingADestructuredObject()
            .WithProperty(innerPropertyName).WithValue(value);

    private static LogEventPropertyValueAssertions WithDeconstructedFakeModelProperty(this LogEventAssertion logEventAssertion,
        string deconstructedPropertyName) =>
        logEventAssertion
            .WithProperty(FakeModelProperty).HavingADestructuredObject()
            .WithProperty(deconstructedPropertyName);
}
