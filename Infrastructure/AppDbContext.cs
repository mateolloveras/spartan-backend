using Microsoft.EntityFrameworkCore;
using SpartanBackend.Infrastructure.Entities;

namespace SpartanBackend.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ScheduleBooking> ScheduleBookings => Set<ScheduleBooking>();
}
