using Microsoft.EntityFrameworkCore;
using SpartanBackend.Infrastructure;

namespace SpartanBackend.Modules.Memberships;

public class MembershipsService(AppDbContext db)
{
    public async Task<object> GetPlansAsync() =>
        await db.MembershipPlans.Where(p => p.IsActive).ToListAsync();

    public async Task<object> GetAllAsync() =>
        await db.Memberships.Include(m => m.User).Include(m => m.Plan).ToListAsync();
}
