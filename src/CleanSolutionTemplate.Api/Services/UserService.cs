using System.Security.Claims;
using CleanSolutionTemplate.Application.Common.Services;

namespace CleanSolutionTemplate.Api.Services;

public class UserService : IUserService
{
    private const string Unknown = nameof(Unknown);

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(IHttpContextAccessor httpContextAccessor)
    {
        this._httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserId() =>
        this._httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue(ClaimTypes.NameIdentifier) ?? Unknown;

    public string GetCurrentUserEmail() =>
        this._httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue(ClaimTypes.Email) ?? Unknown;
}
