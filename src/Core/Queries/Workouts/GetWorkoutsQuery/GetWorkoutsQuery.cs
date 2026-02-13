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

    public async Task<Result<IEnumerable<Workout>>> ExecuteAsync(string userId, CancellationToken cancellationToken)
        => Result<IEnumerable<Workout>>.Success(await _workoutRepository.GetAsync(userId, cancellationToken));
}
