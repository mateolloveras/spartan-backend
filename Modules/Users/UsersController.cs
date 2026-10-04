using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpartanBackend.Modules.Users;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "admin")]
public class UsersController(UsersService usersService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await usersService.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object dto) => Ok(); // TODO: tipear dto

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] object dto) => Ok();

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(Guid id) => Ok();
}
