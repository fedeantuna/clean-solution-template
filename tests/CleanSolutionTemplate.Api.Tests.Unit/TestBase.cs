using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CleanSolutionTemplate.Api.Tests.Unit;

public class TestBase
{
    protected const string TestUserEmail = "test-user-email";
    protected const string TestUserId = "test-user-id";

    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly IServiceProvider _provider;

    protected TestBase()
    {
        this._services.AddPresentationServices(null!);

        this.UnregisterActualHttpContextAccessor();
        this.SetupHttpContextAccessorMock();

        this._provider = this._services.BuildServiceProvider();
    }

    protected Mock<HttpContext> HttpContextMock { get; private set; } = null!;

    protected T FindService<T>()
        where T : notnull
    {
        return this._provider.GetRequiredService<T>();
    }

    private void UnregisterActualHttpContextAccessor()
    {
        var httpContextAccessor = this._services.Single(s => s.ServiceType == typeof(IHttpContextAccessor));
        this._services.Remove(httpContextAccessor);
    }

    private void SetupHttpContextAccessorMock()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, TestUserId),
            new(ClaimTypes.Email, TestUserEmail)
        };

        var claimsIdentity = new ClaimsIdentity(claims);

        var user = new ClaimsPrincipal(claimsIdentity);

        this.HttpContextMock = new Mock<HttpContext>();
        this.HttpContextMock.SetupGet(hc => hc.User).Returns(user);

        httpContextAccessorMock.SetupGet(hca => hca.HttpContext).Returns(this.HttpContextMock.Object);

        this._services.AddSingleton(httpContextAccessorMock.Object);
    }
}
