using System.Security.Claims;
using CleanSolutionTemplate.Application.Common.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CleanSolutionTemplate.Api.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly IUserService _sut;

    public UserServiceTests()
    {
        var provider = new ServiceProviderBuilder().Build();

        this._httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();

        this._sut = provider.GetRequiredService<IUserService>();
    }

    [Fact]
    public void GetCurrentUserId_ReturnsCurrentUserId_WhenUserIdExists()
    {
        // Act
        var result = this._sut.GetCurrentUserId();

        // Assert
        result.Should().Be(Testing.TestUserId);
    }

    [Fact]
    public void GetCurrentUserId_ReturnsCurrentUserEmail_WhenUserEmailExists()
    {
        // Act
        var result = this._sut.GetCurrentUserEmail();

        // Assert
        result.Should().Be(Testing.TestUserEmail);
    }

    [Fact]
    public void GetCurrentUserId_ReturnsUnknown_WhenUserIdDoesNotExists()
    {
        // Arrange
        const string unknownUserId = "Unknown";

        var httpContextMock = Mock.Get(this._httpContextAccessor.HttpContext!);
        httpContextMock.SetupGet(hc => hc.User).Returns(new ClaimsPrincipal());

        // Act
        var result = this._sut.GetCurrentUserId();

        // Assert
        result.Should().Be(unknownUserId);
    }

    [Fact]
    public void GetCurrentUserEmail_ReturnsUnknown_WhenUserEmailDoesNotExists()
    {
        // Arrange
        const string unknownUserEmail = "Unknown";

        var httpContextMock = Mock.Get(this._httpContextAccessor.HttpContext!);
        httpContextMock.SetupGet(hc => hc.User).Returns(new ClaimsPrincipal());

        // Act
        var result = this._sut.GetCurrentUserEmail();

        // Assert
        result.Should().Be(unknownUserEmail);
    }
}
