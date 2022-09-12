using CleanSolutionTemplate.Domain.Common;
using Microsoft.Extensions.Logging;

namespace CleanSolutionTemplate.Application.Tests.Unit.Fakes;

internal class FakeLogger<T> : ILogger<T>
{
    private readonly List<LogRecord> _logRecords = new();

    public string Category { get; } = typeof(T).Name;
    public IEnumerable<LogRecord> LogRecords => this._logRecords;

    public void Log<TState>(LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var logRecord = new LogRecord
        {
            LogLevel = (int)logLevel,
            EventId = eventId.Id,
            State = state?.ToString(),
            Exception = exception
        };
        this._logRecords.Add(logRecord);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return FakeLoggerScope.Instance;
    }
}

internal class LogRecord : ValueObject
{
    public int LogLevel { get; init; }
    public int EventId { get; init; }
    public string? State { get; init; }
    public Exception? Exception { get; init; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return this.LogLevel;
        yield return this.EventId;
        yield return this.State;
        yield return this.Exception?.GetType().ToString();
        yield return this.Exception?.Message;
    }
}

internal sealed class FakeLoggerScope : IDisposable
{
    public static FakeLoggerScope Instance { get; } = new();

    private FakeLoggerScope()
    {
    }

    public void Dispose()
    {
    }
}
