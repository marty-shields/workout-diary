using Core.AggregateRoots;
using Core.Entities;
using Core.Repositories;

namespace Core.Services.Workouts;

internal class WorkoutCreationService : IWorkoutCreationService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IExerciseRepository _exerciseRepository;

    public WorkoutCreationService(
        IWorkoutRepository workoutRepository,
        IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
        _workoutRepository = workoutRepository;
    }

    public async Task<Result<Workout>> CreateWorkoutAsync(WorkoutCreationServiceRequest workout, CancellationToken cancellationToken)
    {
        var distinctExerciseIds = workout.WorkoutActivities.Select(wa => wa.ExerciseId).Distinct();
        var exercisesInDb = await _exerciseRepository.GetExercisesByIdAsync(distinctExerciseIds, cancellationToken);
        if (exercisesInDb.Count() != distinctExerciseIds.Count())
        {
            var missingExerciseIds = distinctExerciseIds.Where(id => !exercisesInDb.Any(e => e.Id == id));
            return Result<Workout>.Failure("Exercise.NotFound", $"Exercises with IDs {string.Join(", ", missingExerciseIds)} not found.");
        }

        var workoutToCreate = workout.ToWorkout(exercisesInDb);
        await _workoutRepository.CreateAsync(workoutToCreate, cancellationToken);
        return Result<Workout>.Success(workoutToCreate);
    }
}
