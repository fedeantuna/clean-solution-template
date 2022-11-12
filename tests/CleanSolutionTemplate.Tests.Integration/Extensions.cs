using System.Net.Http.Headers;

namespace CleanSolutionTemplate.Tests.Integration;

public static class Extensions
{
    public static void WithBearerToken(this HttpClient client, string token)
    {
        const string schema = "Bearer";

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(schema, token);
    }
}
