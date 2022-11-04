using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
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

        this.SetupTestIdentityServerContainer(configuration["Development:LocalhostCertificatePassword"]);

        await this._testIdentityServerContainer.StartAsync();
        this.TestIdentityServerHttpsPort = this._testIdentityServerContainer.GetMappedPublicPort(443);

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
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        return configuration;
    }

    private void SetupTestIdentityServerContainer(string aspnetCoreKestrelCertificatesPassword)
    {
        const string testIdentityServerImage = "fedeantuna/test-identity-server:v1.0.0";
        const string aspnetHttpsContainerDirectory = "/https/";
        var aspnetHttpsHostDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".aspnet",
            "https");

        const string aspnetCoreUrls = "https://+;http://+";
        const string aspnetCoreKestrelCertificatesPath = "/https/localhost.pfx";

        this._testIdentityServerContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage(testIdentityServerImage)
            .WithPortBinding(80, true)
            .WithPortBinding(443, true)
            .WithEnvironment(new Dictionary<string, string>
            {
                { "ASPNETCORE_URLS", aspnetCoreUrls },
                { "ASPNETCORE_Kestrel__Certificates__Default__Password", aspnetCoreKestrelCertificatesPassword },
                { "ASPNETCORE_Kestrel__Certificates__Default__Path", aspnetCoreKestrelCertificatesPath }
            })
            .WithBindMount(aspnetHttpsHostDirectory, aspnetHttpsContainerDirectory)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(443))
            .Build();
    }

    private void UpdateIntegrationTestConfiguration(IConfiguration configuration)
    {
        const string stsAuthoritySettingName = "Sts:Authority";
        configuration[stsAuthoritySettingName] = configuration.GetValue<string>(stsAuthoritySettingName)
            .Replace("#PORT", this.TestIdentityServerHttpsPort.ToString());
    }
}
