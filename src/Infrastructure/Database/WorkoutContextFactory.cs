using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Database;

public class WorkoutContextFactory : IDesignTimeDbContextFactory<WorkoutContext>
{
    public WorkoutContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WorkoutContext>();
        optionsBuilder.UseNpgsql(args[0]);

        return new WorkoutContext(optionsBuilder.Options);
    }
}
