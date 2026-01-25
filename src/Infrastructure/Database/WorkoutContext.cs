using Infrastructure.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class WorkoutContext : DbContext
{
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<Workout> Workouts { get; set; }
    public DbSet<WorkoutExercise> WorkoutExercises { get; set; }

    public WorkoutContext(DbContextOptions<WorkoutContext> options)
        : base(options)
    {
    }
}