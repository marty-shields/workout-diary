using Core.AggregateRoots;
using Core.Repositories;

namespace Core.Queries.Workouts.GetWorkoutsQuery;

public class GetWorkoutsQuery : IGetWorkoutsQuery
{
    private readonly IWorkoutRepository _workoutRepository;

    public GetWorkoutsQuery(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<PaginatedResult<Workout>>> ExecuteAsync(string userId, int pageSize, int pageNumber, CancellationToken cancellationToken)
        => Result<PaginatedResult<Workout>>.Success(await _workoutRepository.GetAsync(userId, pageSize, pageNumber, cancellationToken));
}
