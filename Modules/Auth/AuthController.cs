using Microsoft.AspNetCore.Mvc;
using SpartanBackend.Modules.Auth.DTOs;

namespace SpartanBackend.Modules.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await authService.LoginAsync(dto);
        if (result is null) return Unauthorized(new { message = "Credenciales incorrectas" });
        return Ok(result);
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
    {
        var result = await authService.GoogleLoginAsync(dto.Token);
        if (result is null) return Unauthorized(new { message = "Token de Google inválido" });
        return Ok(result);
    }
}
