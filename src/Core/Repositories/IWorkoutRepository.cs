using Core.AggregateRoots;

namespace Core.Repositories;

public interface IWorkoutRepository
{
    Task CreateAsync(Workout workout, CancellationToken cancellationToken);
    Task<IEnumerable<Workout>> GetAsync(CancellationToken cancellationToken);
    Task<Workout?> GetAsync(Guid workoutId, CancellationToken cancellationToken);
}
