namespace Api.Extensions;

public static class GroupBuilderExtensions
{
    public static RouteGroupBuilder MapWorkoutsApi(this RouteGroupBuilder group)
    {
        group.MapPost("/", Routes.WorkoutRoutes.CreateWorkoutAsync);
        group.MapGet("/", Routes.WorkoutRoutes.GetWorkoutsAsync);
        group.MapGet("/{workoutId:guid}", Routes.WorkoutRoutes.GetWorkoutAsync);
        return group;
    }
}
