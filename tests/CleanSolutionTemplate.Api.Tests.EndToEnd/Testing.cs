using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace CleanSolutionTemplate.Api.Tests.EndToEnd;

public static class Testing
{
    private const int TestIdentityServerContainerPort = 3210;
    private const int TestDatabaseContainerPort = 5433;

    private static readonly HttpClient TestClient = new TestWebApplicationFactory().CreateClient();

    private static IContainer _testIdentityServerContainer = CreateTestIdentityServerContainer();
    private static IContainer _testDatabaseContainer = CreateTestDatabaseContainer();

    public static Task StartTestIdentityServerContainer() => _testIdentityServerContainer.StartAsync();

    public static Task StartTestDatabaseContainer() => _testDatabaseContainer.StartAsync();

    public static Task StopTestIdentityServerContainer() => _testIdentityServerContainer.StopAsync();

    public static Task StopTestDatabaseContainer() => _testDatabaseContainer.StopAsync();

    public static async Task<T> GetDeserializedResponse<T>(HttpResponseMessage response)
    {
        var stringContent = await response.Content.ReadAsStringAsync();
        var deserializedResponse = JsonSerializer.Deserialize<T>(stringContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

        return deserializedResponse;
    }

    public static async Task<string> GetStringResponse(HttpResponseMessage response) =>
        await response.Content.ReadAsStringAsync();

    public static Task<HttpResponseMessage> SendRequest<T>(HttpMethod method, string requestUri, T requestContent)
        where T : class
    {
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri($"{TestClient.BaseAddress!.AbsoluteUri}{requestUri}"),
            Content = new StringContent(JsonSerializer.Serialize(requestContent),
                Encoding.UTF8,
                MediaTypeNames.Application.Json)
        };

        return TestClient.SendAsync(request);
    }

    public static Task<HttpResponseMessage> SendRequest(HttpMethod method, string requestUri)
    {
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri($"{TestClient.BaseAddress!.AbsoluteUri}{requestUri}")
        };

        return TestClient.SendAsync(request);
    }

    public static async Task EnsureRequestIsAuthenticated()
    {
        const string schema = "Bearer";

        const string tokenEndpoint = "http://localhost:3210/connect/token";
        var token = await GetTestIdentityServerToken(tokenEndpoint);

        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(schema, token);
    }

    public static void EnsureRequestIsNotAuthenticated() => TestClient.DefaultRequestHeaders.Authorization = null;

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

    private static IContainer CreateTestIdentityServerContainer()
    {
        const string testIdentityServerImage = "fedeantuna/test-identity-server:v1.0.1";
        const string aspnetCoreUrls = "http://+";

        return _testIdentityServerContainer = new ContainerBuilder()
            .WithImage(testIdentityServerImage)
            .WithPortBinding(TestIdentityServerContainerPort, 80)
            .WithEnvironment(new Dictionary<string, string>
            {
                { "ASPNETCORE_URLS", aspnetCoreUrls }
            })
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80))
            .Build();
    }

    private static IContainer CreateTestDatabaseContainer()
    {
        const string testDatabaseImage = "postgres:15.3-alpine3.18";
        const string testDatabasePassword = "password";

        return _testDatabaseContainer = new ContainerBuilder()
            .WithImage(testDatabaseImage)
            .WithPortBinding(TestDatabaseContainerPort, 5432)
            .WithEnvironment(new Dictionary<string, string>
            {
                { "POSTGRES_PASSWORD", testDatabasePassword }
            })
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    private class IdentityServerResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;
    }
}
