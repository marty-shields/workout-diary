using Core.ValueObjects.Exercise;
using Infrastructure.Database.Tables;
using AggregateRootExercise = Core.AggregateRoots.Exercise;

namespace Infrastructure.Database.ExtensionMethods;

public static class ExerciseExtensionMethods
{
    extension(Exercise exercise)
    {
        public AggregateRootExercise ToEntity()
        {
            return new AggregateRootExercise
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Force = exercise.Force,
                Level = exercise.Level,
                Mechanic = exercise.Mechanic,
                Equipment = exercise.Equipment,
                PrimaryMuscles = exercise.PrimaryMuscles,
                SecondaryMuscles = exercise.SecondaryMuscles,
                Instructions = exercise.Instructions,
                Category = exercise.Category
            };
        }
    }

    extension(List<Exercise> exercises)
    {
        public ICollection<AggregateRootExercise> ToEntities()
        {
            return exercises.Select(e => e.ToEntity()).ToList();
        }
    }

    extension(AggregateRootExercise exercise)
    {
        public Exercise ToTable()
        {
            return new Exercise
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Force = exercise.Force,
                Level = exercise.Level,
                Mechanic = exercise.Mechanic,
                Equipment = exercise.Equipment,
                PrimaryMuscles = exercise.PrimaryMuscles,
                SecondaryMuscles = exercise.SecondaryMuscles,
                Instructions = exercise.Instructions,
                Category = exercise.Category
            };
        }
    }
}
