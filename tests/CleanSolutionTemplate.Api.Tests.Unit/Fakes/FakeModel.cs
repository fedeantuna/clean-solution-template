using System.Diagnostics.CodeAnalysis;
using CleanSolutionTemplate.Domain.Common.Attributes;

namespace CleanSolutionTemplate.Api.Tests.Unit.Fakes;

public class FakeModel
{
    [SensitiveInformation] public List<string> SomeSensitiveStrings { get; init; } = new();

    public List<int> SomeNumbers { get; init; } = new();

    [SensitiveInformation] public string? SomeSensitiveString { get; init; }

    public string? SomeString { get; init; }

    [SensitiveInformation] public decimal SomeSensitiveNumber { get; init; }

    public int SomeNumber { get; init; }

    [SensitiveInformation]
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    public Dictionary<string, int> SomeSensitiveDictionary { get; init; } = new();

    public Dictionary<int, string> SomeDictionary { get; init; } = new();

    [SensitiveInformation] public FakeInnerModelRecord SomeSensitiveFakeInnerModelRecord { get; init; } = new();

    public FakeInnerModelRecord SomeFakeInnerModelRecord { get; init; } = new();
}

public record FakeInnerModelRecord
{
    [SensitiveInformation] public List<int> SomeSensitiveNumbers { get; init; } = new();

    public List<string> SomeStrings { get; init; } = new();

    [SensitiveInformation] public string? SomeSensitiveString { get; init; }

    public string? SomeString { get; init; }

    [SensitiveInformation] public decimal SomeSensitiveNumber { get; init; }

    public int SomeNumber { get; init; }

    [SensitiveInformation]
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    public Dictionary<int, string> SomeSensitiveDictionary { get; init; } = new();

    public Dictionary<string, int> SomeDictionary { get; init; } = new();
}

public class FakeUnreadableModel
{
    private readonly Exception _exception;
    public FakeUnreadableModel(Exception exception)
    {
        this._exception = exception;
    }

    public int ThrowingExceptionProperty =>
        throw this._exception;
}

public class FakeExceptionWithInnerException : Exception
{
    public FakeExceptionWithInnerException(Exception innerException)
        : base(string.Empty, innerException)
    {
    }
}
