using Core.AggregateRoots;

namespace Core.Repositories;

public interface IWorkoutRepository
{
    Task CreateAsync(Workout workout, CancellationToken cancellationToken);
    Task<IEnumerable<Workout>> GetAsync(string userId, CancellationToken cancellationToken);
    Task<Workout?> GetAsync(Guid workoutId, string userId, CancellationToken cancellationToken);
}
