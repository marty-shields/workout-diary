using Core.AggregateRoots;

namespace Core.Queries.Workouts.GetWorkoutsQuery;

public interface IGetWorkoutsQuery
{

    Task<Result<PaginatedResult<Workout>>> ExecuteAsync(string userId, int pageSize, int pageNumber, CancellationToken cancellationToken);
}
