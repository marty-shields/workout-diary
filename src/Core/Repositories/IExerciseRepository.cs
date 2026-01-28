using Core.AggregateRoots;

namespace Core.Repositories;

public interface IExerciseRepository
{
    Task<IEnumerable<Exercise>> GetExercisesByIdAsync(IEnumerable<Guid> exerciseIds, CancellationToken cancellationToken);
}
