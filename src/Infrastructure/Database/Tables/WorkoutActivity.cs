namespace Infrastructure.Database.Tables;

public class WorkoutActivity
{
    public required Guid Id { get; set; }
    public required int Repetitions { get; set; }
    public required double WeightKg { get; set; }
    public required Guid WorkoutId { get; set; }
    public required string WorkoutUserId { get; set; }
    public Workout Workout { get; set; } = null!;
    public required Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
}
