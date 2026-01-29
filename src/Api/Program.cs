using Api.Models.Workouts;
using Core.ExtensionMethods;
using Core.Queries.Workouts.GetWorkoutByIdQuery;
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
builder.Services.AddQueries();

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

    return Results.Created($"/workouts/{result.Value!.Id}", WorkoutResponse.FromEntity(result.Value!));
});

app.MapGet("/workouts/{workoutId:guid}", async (
    [FromServices] IGetWorkoutByIdQuery getWorkoutByIdQuery,
    [FromRoute] Guid workoutId,
    CancellationToken cancellationToken) =>
{
    var result = await getWorkoutByIdQuery.ExecuteAsync(workoutId, cancellationToken);
    if (!result.IsSuccess)
    {
        return Results.NotFound();
    }

    return Results.Ok(WorkoutResponse.FromEntity(result.Value!));
});

app.Run();