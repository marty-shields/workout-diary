using Core.AggregateRoots;

namespace Core.Services.Workouts;

public class WorkoutCreationServiceRequest
{
    public required string UserId { get; init; }
    public string? Notes { get; init; }
    public required int TotalDurationMinutes { get; init; }
    public required DateTimeOffset WorkoutDate { get; init; }
    public required IEnumerable<WorkoutActivity> WorkoutActivities { get; init; }

    public Workout ToWorkout(IEnumerable<Exercise> exercisesInDb)
    {
        var activities = WorkoutActivities.GroupBy(wa => wa.ExerciseId)
            .Select(g => new Entities.WorkoutActivity
            {
                Exercise = exercisesInDb.First(e => e.Id == g.Key),
                Sets = g.Select(wa => new ValueObjects.Workout.WorkoutSet
                {
                    Repetitions = wa.Repetitions,
                    WeightKg = wa.WeightKg
                })
            });

        return new Workout
        {
            Id = Guid.CreateVersion7(),
            UserId = UserId,
            Notes = Notes,
            TotalDurationMinutes = TotalDurationMinutes,
            WorkoutDate = WorkoutDate,
            WorkoutActivities = activities
        };
    }

    public class WorkoutActivity
    {
        public required Guid ExerciseId { get; init; }
        public int Repetitions { get; init; }
        public double WeightKg { get; init; }
    }
}
