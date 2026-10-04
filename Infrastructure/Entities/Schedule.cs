namespace SpartanBackend.Infrastructure.Entities;

public class Schedule
{
    public Guid Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid EmployeeId { get; set; }
    public User Employee { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int MaxCapacity { get; set; }
    public List<ScheduleBooking> Bookings { get; set; } = [];
}

public class ScheduleBooking
{
    public Guid Id { get; set; }
    public Guid ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = null!;
    public Guid ClientId { get; set; }
    public User Client { get; set; } = null!;
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}
