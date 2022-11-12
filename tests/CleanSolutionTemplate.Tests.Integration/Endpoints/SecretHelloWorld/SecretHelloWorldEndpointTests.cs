using System.Net;
using System.Text;
using CleanSolutionTemplate.Api.Endpoints.SecretHelloWorld;
using FluentAssertions;

namespace CleanSolutionTemplate.Tests.Integration.Endpoints.SecretHelloWorld;

public class SecretHelloWorldEndpointTests : TestBase
{
    private const string RequestUri = "api/secret-hello-world";

    public async Task ShouldNotAllowUnauthorizedUsersToAccessProtectedEndpoints()
    {
        this.EnsureRequestIsNotAuthenticated();

        var response = await this.SendRequest(HttpMethod.Get, RequestUri);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldDisplayCorrectMessageForAuthenticatedUser_WhenNoBodyIsSent()
    {
        await this.EnsureRequestIsAuthenticated();

        var response = await this.SendRequest(HttpMethod.Get, RequestUri);

        response.EnsureSuccessStatusCode();

        var secretHelloWorldResponse = await GetDeserializedResponse<SecretHelloWorldResponse>(response);
        var message = secretHelloWorldResponse.Message;
        var secret = Encoding.UTF8.GetString(Convert.FromBase64String(secretHelloWorldResponse.Secret));

        message.Should().Be("Hello World! And Hello Random Person :)");
        secret.Should().Be("Super Special Secret");
    }

    [Test]
    public async Task ShouldDisplayCorrectMessageForAuthenticatedUser_WhenBodyIsSent()
    {
        await this.EnsureRequestIsAuthenticated();

        var secretHelloWorldRequest = CreateSecretHelloWorldRequest();
        var response = await this.SendRequest(HttpMethod.Get,
            RequestUri,
            secretHelloWorldRequest);

        response.EnsureSuccessStatusCode();

        var secretHelloWorldResponse = await GetDeserializedResponse<SecretHelloWorldResponse>(response);
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
