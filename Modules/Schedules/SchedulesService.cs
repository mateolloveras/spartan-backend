using Microsoft.EntityFrameworkCore;
using SpartanBackend.Infrastructure;

namespace SpartanBackend.Modules.Schedules;

public class SchedulesService(AppDbContext db)
{
    public async Task<object> GetAllAsync() =>
        await db.Schedules.Include(s => s.Employee).Include(s => s.Bookings).ToListAsync();
}
