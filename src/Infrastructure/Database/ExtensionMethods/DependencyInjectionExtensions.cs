using Core.Repositories;
using Infrastructure.Database.Repositories;
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
    }
}
