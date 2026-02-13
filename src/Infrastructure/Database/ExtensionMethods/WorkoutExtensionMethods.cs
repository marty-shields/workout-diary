using Infrastructure.Database.Tables;
using AggregateRootWorkout = Core.AggregateRoots.Workout;

namespace Infrastructure.Database.ExtensionMethods;

public static class WorkoutExtensionMethods
{
    extension(Workout workout)
    {
        public AggregateRootWorkout ToEntity()
        {
            return new AggregateRootWorkout
            {
                Id = workout.Id,
                UserId = workout.UserId,
                Notes = workout.Notes,
                TotalDurationMinutes = workout.TotalDurationMinutes,
                WorkoutDate = workout.WorkoutDate,
                WorkoutActivities = workout.WorkoutActivities
                    .GroupBy(activity => activity.ExerciseId)
                    .Select(group => new Core.Entities.WorkoutActivity
                    {
                        Exercise = group.First().Exercise.ToEntity(),
                        Sets = group.Select(activity => new Core.ValueObjects.Workout.WorkoutSet
                        {
                            Repetitions = activity.Repetitions,
                            WeightKg = activity.WeightKg
                        })
                    })
            };
        }
    }

    extension(AggregateRootWorkout workout)
    {
        public Workout ToTable()
        {
            return new Workout
            {
                Id = workout.Id,
                UserId = workout.UserId,
                Notes = workout.Notes,
                TotalDurationMinutes = workout.TotalDurationMinutes,
                WorkoutDate = workout.WorkoutDate
            };
        }

        public IEnumerable<WorkoutActivity> ToTableActivities()
        {
            var workoutActivities = new List<WorkoutActivity>();
            foreach (var wa in workout.WorkoutActivities)
            {
                foreach (var set in wa.Sets)
                {
                    workoutActivities.Add(new WorkoutActivity
                    {
                        Id = Guid.CreateVersion7(),
                        WorkoutId = workout.Id,
                        WorkoutUserId = workout.UserId,
                        ExerciseId = wa.Exercise.Id,
                        Repetitions = set.Repetitions,
                        WeightKg = set.WeightKg
                    });
                }
            }
            return workoutActivities;
        }
    }
}
