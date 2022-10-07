using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CleanSolutionTemplate.Tests.Integration;

public class TestBase
{
    public TestBase()
    {
        var factory = new TestWebApplicationFactory();

        this.TestClient = factory.CreateClient();
    }

    protected HttpClient TestClient { get; }
}
