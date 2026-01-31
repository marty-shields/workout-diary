using Core.AggregateRoots;
using Core.Repositories;

namespace Core.Queries.Workouts.GetWorkoutByIdQuery;

public class GetWorkoutByIdQuery : IGetWorkoutByIdQuery
{
    private readonly IWorkoutRepository _workoutRepository;

    public GetWorkoutByIdQuery(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<Workout>> ExecuteAsync(Guid workoutId, CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetAsync(workoutId, cancellationToken);

        if (workout is null)
        {
            return Result<Workout>.Failure("Workout.NotFound", $"Workout with id {workoutId} not found.");
        }

        return Result<Workout>.Success(workout);
    }
}
