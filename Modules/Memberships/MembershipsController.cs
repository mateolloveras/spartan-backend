using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpartanBackend.Modules.Memberships;

[ApiController]
[Route("api/memberships")]
[Authorize]
public class MembershipsController(MembershipsService membershipsService) : ControllerBase
{
    [HttpGet("plans")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPlans() => Ok(await membershipsService.GetPlansAsync());

    [HttpGet("my")]
    [Authorize(Roles = "client")]
    public async Task<IActionResult> GetMyMembership() => Ok(); // TODO

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll() => Ok(await membershipsService.GetAllAsync());
}
