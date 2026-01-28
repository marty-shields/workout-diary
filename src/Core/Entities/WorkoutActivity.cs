using Core.AggregateRoots;
using Core.ValueObjects.Workout;

namespace Core.Entities;

public class WorkoutActivity
{
    public required Guid Id { get; init; }
    public required Exercise Exercise { get; init; }
    public required IEnumerable<WorkoutSet> Sets { get; init; }
}
