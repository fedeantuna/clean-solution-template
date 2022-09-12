using System.Security.Claims;
using CleanSolutionTemplate.Application.Common.Services;
using FluentAssertions;

namespace CleanSolutionTemplate.Api.Tests.Unit.Services;

public class UserServiceTests : TestBase
{
    private readonly IUserService _sut;

    public UserServiceTests()
    {
        this._sut = this.FindService<IUserService>();
    }

    [Fact]
    public void GetCurrentUserId_ReturnsCurrentUserId_WhenUserIdExists()
    {
        // Act
        var result = this._sut.GetCurrentUserId();

        // Assert
        result.Should().Be(TestUserId);
    }

    [Fact]
    public void GetCurrentUserId_ReturnsCurrentUserEmail_WhenUserEmailExists()
    {
        // Act
        var result = this._sut.GetCurrentUserEmail();

        // Assert
        result.Should().Be(TestUserEmail);
    }

    [Fact]
    public void GetCurrentUserId_ReturnsUnknown_WhenUserIdDoesNotExists()
    {
        // Arrange
        const string unknownUserId = "Unknown";
        this.HttpContextMock.SetupGet(hc => hc.User).Returns(new ClaimsPrincipal());

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
        this.HttpContextMock.SetupGet(hc => hc.User).Returns(new ClaimsPrincipal());

        // Act
        var result = this._sut.GetCurrentUserEmail();

        // Assert
        result.Should().Be(unknownUserEmail);
    }
}
