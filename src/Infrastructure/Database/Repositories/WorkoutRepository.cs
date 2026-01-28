using Core.AggregateRoots;
using Core.Repositories;
using Infrastructure.Database.ExtensionMethods;

namespace Infrastructure.Database.Repositories;

public class WorkoutRepository : IWorkoutRepository
{
    private readonly WorkoutContext _context;

    public WorkoutRepository(WorkoutContext context)
    {
        _context = context;
    }

    public Task CreateAsync(Workout workout, CancellationToken cancellationToken)
    {
        var wt = workout.ToTable();
        var activities = workout.ToTableActivities();
        _context.Workouts.AddAsync(wt, cancellationToken);
        _context.WorkoutActivities.AddRangeAsync(activities, cancellationToken);
        return _context.SaveChangesAsync(cancellationToken);
    }
}
