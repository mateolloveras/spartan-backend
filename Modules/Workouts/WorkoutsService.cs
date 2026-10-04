using Microsoft.EntityFrameworkCore;
using SpartanBackend.Infrastructure;

namespace SpartanBackend.Modules.Workouts;

public class WorkoutsService(AppDbContext db)
{
    public async Task<object> GetAllAsync() =>
        await db.Workouts.Include(w => w.Exercises).ToListAsync();
}
