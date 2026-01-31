using Core.AggregateRoots;
using Core.Repositories;
using Infrastructure.Database.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly WorkoutContext _context;

    public WorkoutRepository(WorkoutContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Workout workout, CancellationToken cancellationToken)
    {
        var wt = workout.ToTable();
        var activities = workout.ToTableActivities();
        await _context.Workouts.AddAsync(wt, cancellationToken);
        await _context.WorkoutActivities.AddRangeAsync(activities, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetAsync(CancellationToken cancellationToken)
    {
        var workouts = await _context.Workouts
            .Include(w => w.WorkoutActivities)
            .ThenInclude(wa => wa.Exercise)
            .OrderByDescending(x => x.WorkoutDate)
            .ToListAsync();
        return workouts.Select(w => w.ToEntity());
    }

    public async Task<Workout?> GetAsync(Guid workoutId, CancellationToken cancellationToken)
    {
        var workoutTable = await _context.Workouts
            .Include(w => w.WorkoutActivities)
            .ThenInclude(wa => wa.Exercise)
            .FirstOrDefaultAsync(w => w.Id == workoutId, cancellationToken);
        return workoutTable?.ToEntity();
    }
}
