namespace Infrastructure.Database.Tables;

public class WorkoutExercise
{
    public required int Id { get; set; }
    public required int Repetitions { get; set; }
    public required double WeightKg { get; set; }
    public required int WorkoutId { get; set; }
    public required Workout Workout { get; set; }
    public required int ExerciseId { get; set; }
    public required Exercise Exercise { get; set; }
}
