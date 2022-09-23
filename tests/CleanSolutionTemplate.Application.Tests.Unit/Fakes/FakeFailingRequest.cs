using MediatR;

namespace CleanSolutionTemplate.Application.Tests.Unit.Fakes;

public class FakeFailingRequest : IRequest<MediatR.Unit>
{
    private const string ExceptionMessage = "Fake Failing Request is Failing!";

    public Exception Exception { get; } = new(ExceptionMessage);
}

public class FakeFailingRequestHandler : IRequestHandler<FakeFailingRequest, MediatR.Unit>
{
    public Task<MediatR.Unit> Handle(FakeFailingRequest request, CancellationToken cancellationToken)
    {
        throw request.Exception;
    }
}
