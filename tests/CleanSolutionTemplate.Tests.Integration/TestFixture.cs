namespace CleanSolutionTemplate.Tests.Integration;

[SetUpFixture]
public class TestFixture
{
    [OneTimeSetUp]
    public async Task RunBeforeAnyTest() => await Testing.StartTestDatabaseContainer();

    [OneTimeTearDown]
    public async Task RunAfterAllTests() => await Testing.StopTestDatabaseContainer();
}
