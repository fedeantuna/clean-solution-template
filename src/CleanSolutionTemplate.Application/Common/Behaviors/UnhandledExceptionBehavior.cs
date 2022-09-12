using System.Diagnostics.CodeAnalysis;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanSolutionTemplate.Application.Common.Behaviors;

public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    internal const string LogMessageTemplate = "CleanSolutionTemplate Request: Unhandled Exception for Request {requestName} {@request}";

    private readonly ILogger _logger;

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor")]
    [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
    public UnhandledExceptionBehavior(ILogger<TRequest> logger)
    {
        this._logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            this._logger.LogError(ex,
                LogMessageTemplate,
                requestName, request);

            throw;
        }
    }
}
