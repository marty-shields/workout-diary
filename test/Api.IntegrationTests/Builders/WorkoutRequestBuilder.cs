using Api.Models.Workouts;

namespace Api.IntegrationTests.Builders;

public class WorkoutRequestBuilder
{
    private Exercise[]? _exercises;
    private string _notes = "Test";
    private int? _totalDurationMinutes = 30;
    private DateTime? _workoutDate = DateTime.UtcNow.AddHours(-1);

    public static WorkoutRequestBuilder Create() => new();

    public WorkoutRequestBuilder WithExercises(params Exercise[] exercises)
    {
        _exercises = exercises.Length > 0 ? exercises : null;
        return this;
    }

    public WorkoutRequestBuilder WithoutExercises()
    {
        _exercises = null;
        return this;
    }

    public WorkoutRequestBuilder WithEmptyExercises()
    {
        _exercises = [];
        return this;
    }

    public WorkoutRequestBuilder WithNotes(string notes)
    {
        _notes = notes;
        return this;
    }

    public WorkoutRequestBuilder WithDuration(int? duration)
    {
        _totalDurationMinutes = duration;
        return this;
    }

    public WorkoutRequestBuilder WithWorkoutDate(DateTime? date)
    {
        _workoutDate = date;
        return this;
    }

    public WorkoutRequest Build() => new()
    {
        Exercises = _exercises,
        Notes = _notes,
        TotalDurationMinutes = _totalDurationMinutes,
        WorkoutDate = _workoutDate
    };
}
