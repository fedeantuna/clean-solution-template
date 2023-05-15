using System.Diagnostics.CodeAnalysis;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace CleanSolutionTemplate.Application.Common.Behaviors;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class UnhandledExceptionBehavior<TRequest, TException> : IRequestExceptionAction<TRequest, TException>
    where TRequest : notnull
    where TException : Exception
{
    internal const string LogMessageTemplate = "CleanSolutionTemplate Request: Unhandled Exception for Request {RequestName} {@Request}";

    private readonly ILogger _logger;

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor")]
    [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
    public UnhandledExceptionBehavior(ILogger<TRequest> logger) => this._logger = logger;

    public Task Execute(TRequest request, TException exception, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        this._logger.LogError(exception,
            LogMessageTemplate,
            requestName, request);

        return Task.CompletedTask;
    }
}
