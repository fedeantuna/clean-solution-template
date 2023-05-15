using System.Diagnostics.CodeAnalysis;
using MediatR;

namespace CleanSolutionTemplate.Application.Tests.Unit.Fakes;

public class FailingRequestFake : IRequest
{
    private const string ExceptionMessage = "Fake Failing Request is Failing!";

    public Exception Exception { get; } = new(ExceptionMessage);
}

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class FailingRequestHandlerFake : IRequestHandler<FailingRequestFake>
{
    public Task Handle(FailingRequestFake request, CancellationToken cancellationToken) => throw request.Exception;
}
