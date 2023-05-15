using FluentAssertions;

namespace CleanSolutionTemplate.Api.Tests.EndToEnd.Endpoints.HealthChecks;

public class HealthcheckEndpointTests
{
    private const string RequestUri = "healthcheck";

    [Test]
    public async Task ShouldDisplayHealthy()
    {
        var response = await Testing.SendRequest(HttpMethod.Get, RequestUri);

        response.EnsureSuccessStatusCode();

        var message = await Testing.GetStringResponse(response);

        message.Should().Be("Healthy");
    }
}
