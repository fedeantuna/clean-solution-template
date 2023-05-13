using System.Diagnostics.CodeAnalysis;
using System.Text;
using FastEndpoints;

namespace CleanSolutionTemplate.Api.Endpoints.SecretHelloWorld;

public class SecretHelloWorldEndpoint : Endpoint<SecretHelloWorldRequest, SecretHelloWorldResponse>
{
    public override void Configure()
    {
        this.Get("/api/secret-hello-world");
    }

    public override async Task HandleAsync(SecretHelloWorldRequest req, CancellationToken ct)
    {
        var message = $"Hello World! And Hello {req.Name} :)";
        var secret = Convert.ToBase64String(Encoding.UTF8.GetBytes(req.Secret));

        await this.SendAsync(new SecretHelloWorldResponse
        {
            Message = message,
            Secret = secret
        }, cancellation: ct);
    }
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public record SecretHelloWorldRequest
{
    public string Name { get; init; } = "Random Person";

    public string Secret { get; init; } = "Super Special Secret";
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record SecretHelloWorldResponse
{
    public string Message { get; init; } = null!;

    public string Secret { get; init; } = null!;
}
