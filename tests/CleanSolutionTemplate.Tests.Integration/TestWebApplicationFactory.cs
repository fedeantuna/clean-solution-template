using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CleanSolutionTemplate.Tests.Integration;

internal class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IConfiguration _configuration;

    public TestWebApplicationFactory(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddConfiguration(this._configuration);
        });
    }
}
