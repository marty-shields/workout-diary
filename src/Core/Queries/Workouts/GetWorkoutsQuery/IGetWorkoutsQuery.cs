using Core.AggregateRoots;

namespace Core.Queries.Workouts.GetWorkoutsQuery;

public interface IGetWorkoutsQuery
{

    Task<Result<IEnumerable<Workout>>> ExecuteAsync(string userId, CancellationToken cancellationToken);
}
