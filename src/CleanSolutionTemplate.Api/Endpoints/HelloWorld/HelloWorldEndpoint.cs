using System.Diagnostics.CodeAnalysis;
using FastEndpoints;

namespace CleanSolutionTemplate.Api.Endpoints.HelloWorld;

public class HelloWorldEndpoint : Endpoint<HelloWorldRequest, HelloWorldResponse>
{
    public override void Configure()
    {
        this.Get("/api/hello-world");
        this.AllowAnonymous();
    }

    public override async Task HandleAsync(HelloWorldRequest req, CancellationToken ct)
    {
        var message = $"Hello World! And Hello {req.Name} :)";

        await this.SendAsync(new HelloWorldResponse
        {
            Message = message
        }, cancellation: ct);
    }
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record HelloWorldRequest
{
    public string Name { get; init; } = "Random Person";
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record HelloWorldResponse
{
    public string Message { get; init; } = null!;
}
