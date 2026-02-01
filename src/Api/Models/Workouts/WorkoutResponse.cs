namespace Api.Models.Workouts;

public class WorkoutResponse
{
    public required Guid Id { get; init; }
    public string? Notes { get; init; }
    public required int TotalDurationMinutes { get; init; }
    public required DateTimeOffset WorkoutDate { get; init; }
    public required IEnumerable<WorkoutActivity> WorkoutActivities { get; init; }

    public static WorkoutResponse FromEntity(Core.AggregateRoots.Workout workout)
    {
        return new WorkoutResponse
        {
            Id = workout.Id,
            Notes = workout.Notes,
            TotalDurationMinutes = workout.TotalDurationMinutes,
            WorkoutDate = workout.WorkoutDate,
            WorkoutActivities = workout.WorkoutActivities.Select(wa => new WorkoutActivity
            {
                ExerciseName = wa.Exercise.Name,
                Sets = wa.Sets.Select(s => new Sets
                {
                    Repetitions = s.Repetitions,
                    WeightKg = s.WeightKg
                })
            })
        };
    }
}

public class WorkoutActivity
{
    public required string ExerciseName { get; init; }
    public required IEnumerable<Sets> Sets { get; init; }
}

public class Sets
{
    public required int Repetitions { get; init; }
    public required double WeightKg { get; init; }
}