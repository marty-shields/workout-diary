using Core.AggregateRoots;

namespace Core.Services.Workouts;

public interface IWorkoutCreationService
{
    Task<Result<Workout>> CreateWorkoutAsync(WorkoutCreationServiceRequest workout, CancellationToken cancellationToken);
}
