using Core.AggregateRoots;
using Core.Repositories;
using Infrastructure.Database.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly WorkoutContext _context;

    public ExerciseRepository(WorkoutContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Exercise>> GetExercisesByIdAsync(IEnumerable<Guid> exerciseIds, CancellationToken cancellationToken)
        => (await _context.Exercises
            .Where(x => exerciseIds.Any(id => id == x.Id))
            .ToListAsync(cancellationToken))
            .ToEntities();
}
