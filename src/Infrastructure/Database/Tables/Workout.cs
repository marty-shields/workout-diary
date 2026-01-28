namespace Infrastructure.Database.Tables;

public class Workout
{
    public required Guid Id { get; set; }
    public string? Notes { get; set; }
    public required int TotalDurationMinutes { get; set; }
    public required DateTime WorkoutDate { get; set; }
    public IEnumerable<WorkoutActivity> WorkoutActivities { get; } = new List<WorkoutActivity>();
}
