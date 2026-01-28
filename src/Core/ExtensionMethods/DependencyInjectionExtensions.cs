using Core.Services.Workouts;
using Microsoft.Extensions.DependencyInjection;

namespace Core.ExtensionMethods;

public static class DependencyInjectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddServices()
        {
            services.AddScoped<IWorkoutCreationService, WorkoutCreationService>();
        }
    }
}
