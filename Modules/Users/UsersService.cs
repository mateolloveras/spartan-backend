using Microsoft.EntityFrameworkCore;
using SpartanBackend.Infrastructure;

namespace SpartanBackend.Modules.Users;

public class UsersService(AppDbContext db)
{
    public async Task<object> GetAllAsync() =>
        await db.Users.Where(u => u.IsActive).Select(u => new {
            u.Id, u.Name, u.Email, Role = u.Role.ToString().ToLower(), u.CreatedAt
        }).ToListAsync();
}
