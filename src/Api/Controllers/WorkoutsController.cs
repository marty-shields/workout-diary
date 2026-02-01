using Api.Models.Workouts;
using Core.Queries.Workouts.GetWorkoutByIdQuery;
using Core.Queries.Workouts.GetWorkoutsQuery;
using Core.Services.Workouts;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
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

    [HttpGet(Name = "GetWorkouts")]
    public async Task<IResult> Get(CancellationToken cancellationToken)
    {
        var result = await getWorkoutsQuery.ExecuteAsync(cancellationToken);
        return Results.Ok(result.Value!.Select(x => WorkoutResponse.FromEntity(x)));
    }

    [HttpGet("{id:guid}", Name = "GetWorkoutById")]
    public async Task<IResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await getWorkoutByIdQuery.ExecuteAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return Results.NotFound();
        }

        return Results.Ok(WorkoutResponse.FromEntity(result.Value!));
    }

    [HttpPost(Name = "CreateWorkout")]
    public async Task<IResult> Post(WorkoutRequest request, CancellationToken cancellationToken)
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
}