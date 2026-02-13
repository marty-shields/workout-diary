using Core.AggregateRoots;
using Core.Queries;

namespace Core.Repositories;

public interface IWorkoutRepository
{
    Task CreateAsync(Workout workout, CancellationToken cancellationToken);
    Task<PaginatedResult<Workout>> GetAsync(string userId, int pageSize, int pageNumber, CancellationToken cancellationToken);
    Task<Workout?> GetAsync(Guid workoutId, string userId, CancellationToken cancellationToken);
}
