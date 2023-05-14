namespace CleanSolutionTemplate.Api.Tests.EndToEnd;

[SetUpFixture]
public class TestFixture
{
    [OneTimeSetUp]
    public async Task RunBeforeAnyTest()
    {
        await Testing.StartTestIdentityServerContainer();
        await Testing.StartTestDatabaseContainer();
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        await Testing.StopTestIdentityServerContainer();
        await Testing.StopTestDatabaseContainer();
    }
}
