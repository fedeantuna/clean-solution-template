using System.Net;
using System.Text;
using CleanSolutionTemplate.Api.Endpoints.SecretHelloWorld;
using FluentAssertions;

namespace CleanSolutionTemplate.Api.Tests.EndToEnd.Endpoints.SecretHelloWorld;

public class SecretHelloWorldEndpointTests
{
    private const string RequestUri = "api/secret-hello-world";

    [Test]
    public async Task ShouldNotAllowUnauthorizedUsersToAccessProtectedEndpoints()
    {
        Testing.EnsureRequestIsNotAuthenticated();

        var response = await Testing.SendRequest(HttpMethod.Get, RequestUri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldDisplayCorrectMessageForAuthenticatedUser_WhenNoBodyIsSent()
    {
        await Testing.EnsureRequestIsAuthenticated();

        var response = await Testing.SendRequest(HttpMethod.Get, RequestUri);

        response.EnsureSuccessStatusCode();

        var secretHelloWorldResponse = await Testing.GetDeserializedResponse<SecretHelloWorldResponse>(response);
        var message = secretHelloWorldResponse.Message;
        var secret = Encoding.UTF8.GetString(Convert.FromBase64String(secretHelloWorldResponse.Secret));

        message.Should().Be("Hello World! And Hello Random Person :)");
        secret.Should().Be("Super Special Secret");
    }

    [Test]
    public async Task ShouldDisplayCorrectMessageForAuthenticatedUser_WhenBodyIsSent()
    {
        await Testing.EnsureRequestIsAuthenticated();

        var secretHelloWorldRequest = CreateSecretHelloWorldRequest();
        var response = await Testing.SendRequest(HttpMethod.Get,
            RequestUri,
            secretHelloWorldRequest);

        response.EnsureSuccessStatusCode();

        var secretHelloWorldResponse = await Testing.GetDeserializedResponse<SecretHelloWorldResponse>(response);
        var message = secretHelloWorldResponse.Message;
        var secret = Encoding.UTF8.GetString(Convert.FromBase64String(secretHelloWorldResponse.Secret));

        message.Should().Be($"Hello World! And Hello {secretHelloWorldRequest.Name} :)");
        secret.Should().Be(secretHelloWorldRequest.Secret);
    }

    private static SecretHelloWorldRequest CreateSecretHelloWorldRequest()
    {
        const string someName = "some-name";
        const string someSecret = "some-secret";

        return new SecretHelloWorldRequest
        {
            Name = someName,
            Secret = someSecret
        };
    }
}
