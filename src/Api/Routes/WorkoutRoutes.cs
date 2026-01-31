using Api.Models.Workouts;
using Core.Queries.Workouts.GetWorkoutByIdQuery;
using Core.Queries.Workouts.GetWorkoutsQuery;
using Core.Services.Workouts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Routes;

public class WorkoutRoutes
{
    public static async Task<IResult> CreateWorkoutAsync(
        [FromServices] IWorkoutCreationService workoutCreationService,
        [FromBody] WorkoutRequest request,
        CancellationToken cancellationToken)
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
    }

    public static async Task<IResult> GetWorkoutAsync(
        [FromServices] IGetWorkoutByIdQuery getWorkoutByIdQuery,
        [FromRoute] Guid workoutId,
        CancellationToken cancellationToken)
    {
        var result = await getWorkoutByIdQuery.ExecuteAsync(workoutId, cancellationToken);
        if (!result.IsSuccess)
        {
            return Results.NotFound();
        }

        return Results.Ok(WorkoutResponse.FromEntity(result.Value!));
    }

    public static async Task<IResult> GetWorkoutsAsync(
        [FromServices] IGetWorkoutsQuery getWorkoutsQuery,
        CancellationToken cancellationToken)
    {
        var result = await getWorkoutsQuery.ExecuteAsync(cancellationToken);
        return Results.Ok(result.Value!.Select(x => WorkoutResponse.FromEntity(x)));
    }
}
