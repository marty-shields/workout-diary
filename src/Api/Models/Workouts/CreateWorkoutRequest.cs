using System.ComponentModel.DataAnnotations;
using Core.Services.Workouts;

namespace Api.Models.Workouts;

public class WorkoutRequest : IValidatableObject
{
    [Required, MinLength(1)]
    public required Exercise[] Exercises { get; init; }

    public string? Notes { get; init; }

    [Required, Range(1, int.MaxValue)]
    public required int TotalDurationMinutes { get; init; }

    [Required]
    public required DateTimeOffset WorkoutDate { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // validate the WorkoutDate is not in the future
        if (WorkoutDate > DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "Workout date cannot be in the future.",
                [nameof(WorkoutDate)]);
        }

        if (Exercises != null)
        {
            for (int i = 0; i < Exercises.Length; i++)
            {
                var exercise = Exercises[i];
                var context = new ValidationContext(exercise);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(exercise, context, results, true))
                {
                    foreach (var validationResult in results)
                    {
                        var memberNames = validationResult.MemberNames
                            .Select(name => $"Exercises[{i}].{name}");
                        yield return new ValidationResult(
                            validationResult.ErrorMessage!,
                            memberNames);
                    }
                }
            }
        }
    }

    public WorkoutCreationServiceRequest ToWorkoutCreationServiceRequest(string userId)
    {
        var workoutActivities = new List<WorkoutCreationServiceRequest.WorkoutActivity>();
        foreach (var group in Exercises!.GroupBy(e => e.Id))
        {
            foreach (var exercise in group)
            {
                foreach (var set in exercise.Sets!)
                {
                    workoutActivities.Add(new WorkoutCreationServiceRequest.WorkoutActivity
                    {
                        ExerciseId = exercise.Id,
                        Repetitions = set.Repetitions,
                        WeightKg = set.WeightKg
                    });
                }
            }

        }

        return new WorkoutCreationServiceRequest
        {
            UserId = userId,
            Notes = Notes,
            TotalDurationMinutes = TotalDurationMinutes,
            WorkoutDate = WorkoutDate,
            WorkoutActivities = workoutActivities
        };
    }
}

public class Exercise : IValidatableObject
{
    [Required]
    public required Guid Id { get; init; }

    [Required, MinLength(1)]
    public required WorkoutSet[] Sets { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // for some reason when adding validation results the index is not captured correctly as it
        // iterates through the sets, so we need to track it manually
        int level = -1;
        for (int i = 0; i < Sets?.Length; i++)
        {
            level++;
            var set = Sets[i];
            var context = new ValidationContext(set);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(set, context, results, true))
            {
                foreach (var validationResult in results)
                {
                    var memberNames = validationResult.MemberNames
                        .Select(name => $"Sets[{level}].{name}");
                    yield return new ValidationResult(
                        validationResult.ErrorMessage!,
                        memberNames);
                }
            }
        }
    }
}

public class WorkoutSet
{
    [Required, Range(1, int.MaxValue)]
    public required int Repetitions { get; init; }

    [Required, Range(0, double.MaxValue)]
    public required double WeightKg { get; init; }
}