using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;

namespace CleanSolutionTemplate.Tests.Integration;

public class TestBase
{
    private TestcontainersContainer _testIdentityServerContainer = null!;

    protected HttpClient TestClient { get; private set; } = null!;
    protected int TestIdentityServerHttpsPort { get; private set; }

    [OneTimeSetUp]
    public async Task RunBeforeAnyTest()
    {
        var configuration = GetIntegrationTestConfiguration();
        var factory = new TestWebApplicationFactory(configuration);

        this.SetupTestIdentityServerContainer();

        await this._testIdentityServerContainer.StartAsync();
        this.TestIdentityServerHttpsPort = this._testIdentityServerContainer.GetMappedPublicPort(80);

        this.UpdateIntegrationTestConfiguration(configuration);

        this.TestClient = factory.CreateClient();
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        await this._testIdentityServerContainer.StopAsync();
    }

    protected static async Task<T> GetDeserializedResponse<T>(HttpResponseMessage response)
    {
        var stringContent = await response.Content.ReadAsStringAsync();
        var deserializedResponse = JsonSerializer.Deserialize<T>(stringContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return deserializedResponse;
    }

    protected Task<HttpResponseMessage> SendRequest<T>(HttpMethod method, string requestUri, T requestContent)
        where T : class
    {
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri($"{this.TestClient.BaseAddress!.AbsoluteUri}{requestUri}"),
            Content = new StringContent(JsonSerializer.Serialize(requestContent),
                Encoding.UTF8,
                MediaTypeNames.Application.Json)
        };

        return this.TestClient.SendAsync(request);
    }

    protected Task<HttpResponseMessage> SendRequest(HttpMethod method, string requestUri)
    {
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri($"{this.TestClient.BaseAddress!.AbsoluteUri}{requestUri}")
        };

        return this.TestClient.SendAsync(request);
    }

    protected async Task EnsureRequestIsAuthenticated()
    {
        const string schema = "Bearer";

        var tokenEndpoint = $"http://localhost:{this.TestIdentityServerHttpsPort}/connect/token";
        var token = await GetTestIdentityServerToken(tokenEndpoint);

        this.TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(schema, token);
    }

    protected void EnsureRequestIsNotAuthenticated() =>
        this.TestClient.DefaultRequestHeaders.Authorization = null;

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

    private static IConfiguration GetIntegrationTestConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        return configuration;
    }

    private void SetupTestIdentityServerContainer()
    {
        const string testIdentityServerImage = "fedeantuna/test-identity-server:v1.0.1";
        const string aspnetCoreUrls = "http://+";

        this._testIdentityServerContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage(testIdentityServerImage)
            .WithPortBinding(80, true)
            .WithEnvironment(new Dictionary<string, string>
            {
                { "ASPNETCORE_URLS", aspnetCoreUrls },
            })
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
            .Build();
    }

    private void UpdateIntegrationTestConfiguration(IConfiguration configuration)
    {
        const string stsAuthoritySettingName = "Sts:Authority";
        configuration[stsAuthoritySettingName] = configuration.GetValue<string>(stsAuthoritySettingName)!
            .Replace("#PORT", this.TestIdentityServerHttpsPort.ToString());
    }

    private class IdentityServerResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;
    }
}
