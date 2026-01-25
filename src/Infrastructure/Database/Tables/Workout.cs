namespace Infrastructure.Database.Tables;

public class Workout
{
    public required int Id { get; set; }
    public string? Notes { get; set; }
    public required int TotalDurationMinutes { get; set; }
    public required DateTime WorkoutDate { get; set; }
    public required List<WorkoutExercise> WorkoutExercises { get; set; }
}
