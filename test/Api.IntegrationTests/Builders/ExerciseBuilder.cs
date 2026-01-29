using Api.Models.Workouts;

namespace Api.IntegrationTests.Builders;

public class ExerciseBuilder
{
    private Guid? _id = Guid.NewGuid();
    private WorkoutSet[]? _sets = [WorkoutSetBuilder.Create().Build()];

    public static ExerciseBuilder Create() => new();

    public ExerciseBuilder WithId(Guid? id)
    {
        _id = id;
        return this;
    }

    public ExerciseBuilder WithSets(params WorkoutSet[] sets)
    {
        _sets = sets.Length > 0 ? sets : null;
        return this;
    }

    public ExerciseBuilder WithoutSets()
    {
        _sets = null;
        return this;
    }

    public ExerciseBuilder WithEmptySets()
    {
        _sets = [];
        return this;
    }

    public Exercise Build() => new()
    {
        Id = _id,
        Sets = _sets
    };
}
