using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpartanBackend.Modules.Schedules;

[ApiController]
[Route("api/schedules")]
[Authorize]
public class SchedulesController(SchedulesService schedulesService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await schedulesService.GetAllAsync());

    [HttpPost("{id}/book")]
    [Authorize(Roles = "client")]
    public async Task<IActionResult> Book(Guid id) => Ok(); // TODO

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] object dto) => Ok(); // TODO
}
