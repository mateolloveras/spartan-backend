using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpartanBackend.Modules.Workouts;

[ApiController]
[Route("api/workouts")]
[Authorize]
public class WorkoutsController(WorkoutsService workoutsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await workoutsService.GetAllAsync());

    [HttpPost]
    [Authorize(Roles = "admin,employee")]
    public async Task<IActionResult> Create([FromBody] object dto) => Ok(); // TODO
}
