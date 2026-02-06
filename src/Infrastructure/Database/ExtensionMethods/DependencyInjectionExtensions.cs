using Core.Repositories;
using Infrastructure.Database.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

    extension(WebApplication app)
    {
        public void SetupDatabase()
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<WorkoutContext>();
                    context.Database.EnsureCreated();
                }
            }
        }
    }
}
