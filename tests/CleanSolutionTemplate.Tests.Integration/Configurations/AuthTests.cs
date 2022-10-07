using System.Net;
using FluentAssertions;

namespace CleanSolutionTemplate.Tests.Integration.Configurations;

public class AuthTests : TestBase
{
    [Test]
    public async Task ShouldAllowAnyoneToAccessAnonymousEndpoints()
    {
        var response = await this.TestClient.GetAsync("fake/anonymous");

        response.EnsureSuccessStatusCode();

        var stringContent = await response.Content.ReadAsStringAsync();

        stringContent.Should().Be("ANONYMOUS");
    }

    [Test]
    public async Task ShouldNotAllowUnauthorizedUsersToAccessProtectedEndpoints()
    {
        var response = await this.TestClient.GetAsync("fake/default-auth");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
