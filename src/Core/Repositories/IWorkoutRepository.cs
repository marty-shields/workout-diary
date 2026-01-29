using Core.AggregateRoots;

namespace Core.Repositories;

public interface IWorkoutRepository
{
    Task CreateAsync(Workout workout, CancellationToken cancellationToken);
    Task<Workout?> GetByIdAsync(Guid workoutId, CancellationToken cancellationToken);
}
