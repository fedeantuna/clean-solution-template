using System.Diagnostics.CodeAnalysis;
using MediatR;

namespace CleanSolutionTemplate.Application.Tests.Unit.Fakes;

public class ValidatedPassingRequestFake : IRequest
{
    public string? SomeString { get; init; }
}

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class PassingRequestFakeHandler : IRequestHandler<ValidatedPassingRequestFake>
{
    public Task Handle(ValidatedPassingRequestFake request, CancellationToken cancellationToken) => Task.CompletedTask;
}
