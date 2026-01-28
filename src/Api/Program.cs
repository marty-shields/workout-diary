using Api.Models.Workouts;
using Core.ExtensionMethods;
using Core.Services.Workouts;
using Infrastructure.Database;
using Infrastructure.Database.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();
builder.Services.AddDbContext<WorkoutContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("WorkoutContext"));
});
builder.Services.AddRepositories();
builder.Services.AddServices();

WebApplication app = builder.Build();
app.UseHttpsRedirection();

app.MapPost("/workouts", async (
    [FromServices] IWorkoutCreationService workoutCreationService,
    [FromBody] WorkoutRequest request,
    CancellationToken cancellationToken) =>
{
    var result = await workoutCreationService.CreateWorkoutAsync(request.ToWorkoutCreationServiceRequest(), cancellationToken);
    if (!result.IsSuccess)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { "Exercises", new[] { result.Error!.Message } }
        });
    }

    return Results.Created();
})
.Produces(StatusCodes.Status201Created)
.ProducesValidationProblem();

app.Run();