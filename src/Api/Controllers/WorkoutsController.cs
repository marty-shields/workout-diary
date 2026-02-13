using Api.Models.Workouts;
using Core.Queries.Workouts.GetWorkoutByIdQuery;
using Core.Queries.Workouts.GetWorkoutsQuery;
using Core.Services.Workouts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class WorkoutsController : ControllerBase
{
    private readonly IGetWorkoutsQuery getWorkoutsQuery;
    private readonly IGetWorkoutByIdQuery getWorkoutByIdQuery;
    private readonly IWorkoutCreationService workoutCreationService;

    public WorkoutsController(
        IGetWorkoutsQuery getWorkoutsQuery,
        IGetWorkoutByIdQuery getWorkoutByIdQuery,
        IWorkoutCreationService workoutCreationService)
    {
        this.getWorkoutsQuery = getWorkoutsQuery;
        this.getWorkoutByIdQuery = getWorkoutByIdQuery;
        this.workoutCreationService = workoutCreationService;
    }

    private string? GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    [HttpGet(Name = "GetWorkouts")]
    public async Task<IResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Results.Unauthorized();
        var result = await getWorkoutsQuery.ExecuteAsync(userId, cancellationToken);
        return Results.Ok(result.Value!.Select(x => WorkoutResponse.FromEntity(x)));
    }

    [HttpGet("{id:guid}", Name = "GetWorkoutById")]
    public async Task<IResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Results.Unauthorized();
        var result = await getWorkoutByIdQuery.ExecuteAsync(id, userId, cancellationToken);
        if (!result.IsSuccess)
        {
            return Results.NotFound();
        }

        return Results.Ok(WorkoutResponse.FromEntity(result.Value!));
    }

    [HttpPost(Name = "CreateWorkout")]
    public async Task<IResult> Post(WorkoutRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Results.Unauthorized();
        var result = await workoutCreationService.CreateWorkoutAsync(request.ToWorkoutCreationServiceRequest(userId), cancellationToken);
        if (!result.IsSuccess)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Exercises", new[] { result.Error!.Message } }
                });
        }

        return Results.Created($"/workouts/{result.Value!.Id}", WorkoutResponse.FromEntity(result.Value!));
    }
}