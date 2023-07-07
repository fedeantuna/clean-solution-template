using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CleanSolutionTemplate.Api.Tests.EndToEnd;

public static class Testing
{
    private static readonly HttpClient TestClient = new TestWebApplicationFactory().CreateClient();

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

    private class IdentityServerResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;
    }
}
