using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanSolutionTemplate.Tests.Integration.Fakes;

public class FakeController : Controller
{
    [Route("/fake/default-auth")]
    public IActionResult DefaultAuth()
    {
        return this.Ok("OK");
    }

    [Route("/fake/anonymous")]
    [AllowAnonymous]
    public IActionResult Anonymous()
    {
        return this.Ok("ANONYMOUS");
    }
}
