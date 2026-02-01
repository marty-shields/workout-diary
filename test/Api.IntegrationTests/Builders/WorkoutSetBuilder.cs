using Api.Models.Workouts;

namespace Api.IntegrationTests.Builders;

public class WorkoutSetBuilder
{
    private int _repetitions = 10;
    private double _weightKg = 5.0;

    public static WorkoutSetBuilder Create() => new();

    public WorkoutSetBuilder WithRepetitions(int repetitions)
    {
        _repetitions = repetitions;
        return this;
    }

    public WorkoutSetBuilder WithWeightKg(double weightKg)
    {
        _weightKg = weightKg;
        return this;
    }

    public WorkoutSet Build() => new()
    {
        Repetitions = _repetitions,
        WeightKg = _weightKg
    };
}
