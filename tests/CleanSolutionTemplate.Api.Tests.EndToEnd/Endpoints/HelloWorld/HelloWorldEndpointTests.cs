using CleanSolutionTemplate.Api.Endpoints.HelloWorld;
using FluentAssertions;

namespace CleanSolutionTemplate.Api.Tests.EndToEnd.Endpoints.HelloWorld;

public class HelloWorldEndpointTests
{
    private const string RequestUri = "api/hello-world";

    [Test]
    public async Task ShouldDisplayCorrectMessageForAnonymousUser_WhenNoBodyIsSent()
    {
        var response = await Testing.SendRequest(HttpMethod.Get, RequestUri);

        response.EnsureSuccessStatusCode();

        var helloWorldResponse = await Testing.GetDeserializedResponse<HelloWorldResponse>(response);
        var message = helloWorldResponse.Message;

        message.Should().Be("Hello World! And Hello Random Person :)");
    }

    [Test]
    public async Task ShouldDisplayCorrectMessageForAnonymousUser_WhenBodyIsSent()
    {
        var helloWorldRequest = CreateHelloWorldRequest();
        var response = await Testing.SendRequest(HttpMethod.Get,
            RequestUri,
            helloWorldRequest);

        response.EnsureSuccessStatusCode();

        var helloWorldResponse = await Testing.GetDeserializedResponse<HelloWorldResponse>(response);
        var message = helloWorldResponse.Message;

        message.Should().Be($"Hello World! And Hello {helloWorldRequest.Name} :)");
    }

    private static HelloWorldRequest CreateHelloWorldRequest()
    {
        const string someName = "some-name";
        return new HelloWorldRequest
        {
            Name = someName
        };
    }
}
