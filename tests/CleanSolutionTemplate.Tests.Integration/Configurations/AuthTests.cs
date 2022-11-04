using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;

namespace CleanSolutionTemplate.Tests.Integration.Configurations;

public class AuthTests : TestBase
{
    [Test, Order(1)]
    public async Task ShouldAllowAnyoneToAccessAnonymousEndpoints()
    {
        var response = await this.TestClient.GetAsync("fake/anonymous");

        response.EnsureSuccessStatusCode();

        var stringContent = await response.Content.ReadAsStringAsync();

        stringContent.Should().Be("ANONYMOUS");
    }

    [Test, Order(2)]
    public async Task ShouldNotAllowUnauthorizedUsersToAccessProtectedEndpoints()
    {
        var response = await this.TestClient.GetAsync("fake/default-auth");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test, Order(3)]
    public async Task ShouldAllowAuthorizedUsersToAccessProtectedEndpoints()
    {
        const string schema = "Bearer";
        var tokenEndpoint = $"http://localhost:{this.TestIdentityServerHttpsPort}/connect/token";
        var token = await GetTestIdentityServerToken(tokenEndpoint);

        this.TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(schema, token);

        var response = await this.TestClient.GetAsync("fake/default-auth");

        response.EnsureSuccessStatusCode();

        var stringContent = await response.Content.ReadAsStringAsync();

        stringContent.Should().Be("OK");
    }

    private static async Task<string> GetTestIdentityServerToken(string tokenEndpoint)
    {
        using var httpClient = new HttpClient();
        using var identityRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
        var contentList = new List<string>
        {
            "client_id=test-client",
            "client_secret=test-client-secret",
            "scope=test-scope",
            "grant_type=client_credentials"
        };
        identityRequest.Content = new StringContent(string.Join("&", contentList));
        identityRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

        var identityResponse = await httpClient.SendAsync(identityRequest);
        var serializedIdentityServerResponse = await identityResponse.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<IdentityServerResponse>(serializedIdentityServerResponse)!.AccessToken;
    }
}

public class IdentityServerResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;
}
