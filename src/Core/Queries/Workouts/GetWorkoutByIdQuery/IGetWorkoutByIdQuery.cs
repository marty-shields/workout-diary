using Core.AggregateRoots;

namespace Core.Queries.Workouts.GetWorkoutByIdQuery;

public interface IGetWorkoutByIdQuery
{
    Task<Result<Workout>> ExecuteAsync(Guid workoutId, CancellationToken cancellationToken);
}
