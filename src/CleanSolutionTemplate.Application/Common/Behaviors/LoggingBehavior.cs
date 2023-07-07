using System.Diagnostics.CodeAnalysis;
using CleanSolutionTemplate.Application.Common.Services;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace CleanSolutionTemplate.Application.Common.Behaviors;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class LoggingBehavior<TRequest> : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    internal const string LogMessageTemplate = "Request: {RequestName} {UserId} {@Request}";

    private readonly ILogger _logger;
    private readonly IUserService _userService;

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor")]
    [SuppressMessage("ReSharper", "ContextualLoggerProblem")]
    public LoggingBehavior(ILogger<TRequest> logger,
        IUserService userService)
    {
        this._logger = logger;
        this._userService = userService;
    }

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = this._userService.GetCurrentUserId();

        this._logger.LogInformation(LogMessageTemplate,
            requestName, userId, request);

        return Task.CompletedTask;
    }
}
