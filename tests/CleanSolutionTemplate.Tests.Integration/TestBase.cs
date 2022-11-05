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
        configuration[stsAuthoritySettingName] = configuration.GetValue<string>(stsAuthoritySettingName)
            .Replace("#PORT", this.TestIdentityServerHttpsPort.ToString());
    }
}
