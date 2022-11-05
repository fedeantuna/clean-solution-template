using System.Reflection;
using CleanSolutionTemplate.Tests.Integration.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        builder.ConfigureServices(services =>
        {
            services.AddMvc()
                .AddApplicationPart(Assembly.GetExecutingAssembly())
                .AddControllersAsServices();
            services.AddScoped(_ => new FakeController());
        });
    }
}
