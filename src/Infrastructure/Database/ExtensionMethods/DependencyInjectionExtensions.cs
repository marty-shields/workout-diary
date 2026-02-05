using Core.Repositories;
using Infrastructure.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database.ExtensionMethods;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddRepositories()
        {
            services.AddScoped<IWorkoutRepository, WorkoutRepository>();
            services.AddScoped<IExerciseRepository, ExerciseRepository>();
        }

        public void AddDbContext(string connectionString)
        {
            services.AddDbContext<WorkoutContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
        }
    }

    extension(IHealthChecksBuilder healthChecksBuilder)
    {
        public void AddDbContextHealthCheck()
        {
            healthChecksBuilder.AddDbContextCheck<WorkoutContext>();
        }
    }
}
