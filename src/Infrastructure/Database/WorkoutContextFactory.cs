using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Database;

public class WorkoutContextFactory : IDesignTimeDbContextFactory<WorkoutContext>
{
    public WorkoutContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WorkoutContext>()
            .UseSqlite("Data Source=workout.db")
            .Options;

        return new WorkoutContext(options);
    }
}
