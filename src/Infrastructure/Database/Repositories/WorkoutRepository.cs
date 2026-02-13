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

    public async Task<IEnumerable<Workout>> GetAsync(string userId, CancellationToken cancellationToken)
    {
        var workouts = await _context.Workouts
            .Where(w => w.UserId == userId)
            .Include(w => w.WorkoutActivities)
            .ThenInclude(wa => wa.Exercise)
            .OrderByDescending(x => x.WorkoutDate)
            .ToListAsync(cancellationToken);
        return workouts.Select(w => w.ToEntity());
    }

    public async Task<Workout?> GetAsync(Guid workoutId, string userId, CancellationToken cancellationToken)
    {
        var workoutTable = await _context.Workouts
            .Include(w => w.WorkoutActivities)
            .ThenInclude(wa => wa.Exercise)
            .FirstOrDefaultAsync(
                w => w.Id.Equals(workoutId) && w.UserId.Equals(userId),
                cancellationToken);
        return workoutTable?.ToEntity();
    }
}
