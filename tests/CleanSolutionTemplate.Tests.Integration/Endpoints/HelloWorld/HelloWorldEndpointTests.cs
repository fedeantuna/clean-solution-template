using CleanSolutionTemplate.Api.Endpoints.HelloWorld;
using FluentAssertions;

namespace CleanSolutionTemplate.Tests.Integration.Endpoints.HelloWorld;

public class HelloWorldEndpointTests : TestBase
{
    private const string RequestUri = "api/hello-world";

    [Test]
    public async Task ShouldDisplayCorrectMessageForAnonymousUser_WhenNoBodyIsSent()
    {
        var response = await this.SendRequest(HttpMethod.Get, RequestUri);

        response.EnsureSuccessStatusCode();

        var helloWorldResponse = await GetDeserializedResponse<HelloWorldResponse>(response);
        var message = helloWorldResponse.Message;

        message.Should().Be("Hello World! And Hello Random Person :)");
    }

    [Test]
    public async Task ShouldDisplayCorrectMessageForAnonymousUser_WhenBodyIsSent()
    {
        var helloWorldRequest = CreateHelloWorldRequest();
        var response = await this.SendRequest(HttpMethod.Get,
            RequestUri,
            helloWorldRequest);

        response.EnsureSuccessStatusCode();

        var helloWorldResponse = await GetDeserializedResponse<HelloWorldResponse>(response);
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
