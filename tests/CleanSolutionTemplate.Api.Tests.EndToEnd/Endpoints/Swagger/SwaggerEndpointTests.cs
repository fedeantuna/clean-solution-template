using System.Net;
using FluentAssertions;

namespace CleanSolutionTemplate.Api.Tests.EndToEnd.Endpoints.Swagger;

public class SwaggerEndpointTests
{
    private const string RequestUri = "swagger/v1/swagger.json";

    [Test]
    public async Task ShouldReturnSuccessStatusCode()
    {
        var response = await Testing.SendRequest(HttpMethod.Get, RequestUri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
