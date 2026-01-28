using Core.ValueObjects.Exercise;

namespace Core.AggregateRoots;

public class Exercise
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public Force? Force { get; init; }
    public required Level Level { get; init; }
    public Mechanic? Mechanic { get; init; }
    public Equipment? Equipment { get; init; }
    public required IEnumerable<Muscle> PrimaryMuscles { get; init; }
    public IEnumerable<Muscle>? SecondaryMuscles { get; init; }
    public required IEnumerable<string> Instructions { get; init; }
    public required Category Category { get; init; }
}
