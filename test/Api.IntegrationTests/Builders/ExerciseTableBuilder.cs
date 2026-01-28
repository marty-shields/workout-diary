namespace Api.IntegrationTests.Builders;

public class ExerciseTableBuilder
{
    private Infrastructure.Database.Tables.Exercise _exercise = new Infrastructure.Database.Tables.Exercise
    {
        Id = Guid.NewGuid(),
        Name = "Default Exercise",
        Force = Core.ValueObjects.Exercise.Force.Push,
        Level = Core.ValueObjects.Exercise.Level.Beginner,
        Mechanic = Core.ValueObjects.Exercise.Mechanic.Compound,
        Equipment = Core.ValueObjects.Exercise.Equipment.Bands,
        PrimaryMuscles = new List<Core.ValueObjects.Exercise.Muscle> { Core.ValueObjects.Exercise.Muscle.Chest },
        SecondaryMuscles = new List<Core.ValueObjects.Exercise.Muscle> { Core.ValueObjects.Exercise.Muscle.Triceps },
        Instructions = new List<string> { "Do the exercise properly." },
        Category = Core.ValueObjects.Exercise.Category.Strength
    };

    public static ExerciseTableBuilder Create()
    {
        return new ExerciseTableBuilder();
    }

    public ExerciseTableBuilder WithId(Guid id)
    {
        _exercise.Id = id;
        return this;
    }

    public ExerciseTableBuilder WithName(string name)
    {
        _exercise.Name = name;
        return this;
    }

    public Infrastructure.Database.Tables.Exercise Build()
    {
        return _exercise;
    }
}
