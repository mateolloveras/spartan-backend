using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpartanBackend.Infrastructure;
using SpartanBackend.Infrastructure.Entities;
using SpartanBackend.Modules.Auth.DTOs;

namespace SpartanBackend.Modules.Auth;

public class AuthService(AppDbContext db, IConfiguration config)
{
    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);
        if (user is null || user.PasswordHash is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;
        return BuildResponse(user);
    }

    public async Task<LoginResponseDto?> GoogleLoginAsync(string googleToken)
    {
        // TODO: validar googleToken con Google API y obtener email
        // var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);
        // var user = await db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email && u.IsActive);
        // if (user is null) return null;
        // return BuildResponse(user);
        await Task.CompletedTask;
        return null;
    }

    private LoginResponseDto BuildResponse(User user)
    {
        var token = GenerateJwt(user);
        return new LoginResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.ToString().ToLower()
            }
        };
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:ExpiresInMinutes"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim("role", user.Role.ToString().ToLower())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
