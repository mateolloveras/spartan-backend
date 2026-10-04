namespace SpartanBackend.Infrastructure.Entities;

public class Workout
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<Exercise> Exercises { get; set; } = [];
    public Guid CreatedByEmployeeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Exercise
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sets { get; set; } = string.Empty;
    public string Reps { get; set; } = string.Empty;
    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;
}
