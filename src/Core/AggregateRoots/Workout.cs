using Core.Entities;

namespace Core.AggregateRoots;

public class Workout
{
    public required Guid Id { get; init; }
    public required string UserId { get; init; }
    public string? Notes { get; init; }
    public required int TotalDurationMinutes { get; init; }
    public required DateTimeOffset WorkoutDate { get; init; }
    public required IEnumerable<WorkoutActivity> WorkoutActivities { get; init; }
}
