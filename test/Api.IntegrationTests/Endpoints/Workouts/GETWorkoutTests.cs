using Api.IntegrationTests.Builders;
using Api.Models.Workouts;
using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Api.IntegrationTests.Endpoints.Workouts;

public class GETWorkoutTests : BaseTestFixture
{
    #region Validation Tests - Invalid ID Format

    [Test]
    public async Task WhenWorkoutIdIsInvalidFormat_ShouldReturnNotFound()
    {
        var response = await GetWorkout("invalid-guid");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region Validation Tests - Workout Not Found

    [Test]
    public async Task WhenWorkoutDoesNotExist_ShouldReturnNotFound()
    {
        var nonExistentId = Guid.CreateVersion7();
        var response = await GetWorkout(nonExistentId.ToString());

        Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.NotFound));
    }

    #endregion

    #region Happy Path Tests

    [Test]
    public async Task WhenWorkoutExists_ShouldReturnOkWithWorkoutData()
    {
        var exerciseId = Guid.CreateVersion7();
        var workoutId = Guid.CreateVersion7();
        var workoutDate = DateTimeOffset.UtcNow.AddHours(-2);

        await SeedExercise(exerciseId, "Pushups");
        await SeedWorkoutWithSingleExercise(workoutId, exerciseId, workoutDate);

        var response = await GetWorkout(workoutId.ToString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected OK but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");

        var workout = await ParseWorkoutResponse(response);
        Assert.That(workout, Is.Not.Null);
        Assert.That(workout!.Id, Is.EqualTo(workoutId));
        Assert.That(workout.Notes, Is.EqualTo("Single Exercise Workout"));
        Assert.That(workout.TotalDurationMinutes, Is.EqualTo(30));
        Assert.That(workout.WorkoutDate, Is.EqualTo(workoutDate));
        Assert.That(workout.WorkoutActivities.Count(), Is.EqualTo(1));
        var activity = workout.WorkoutActivities.First();
        Assert.That(activity.ExerciseName, Is.EqualTo("Pushups"));
        Assert.That(activity.Sets.Count(), Is.EqualTo(1));
        var set = activity.Sets.First();
        Assert.That(set.Repetitions, Is.EqualTo(10));
        Assert.That(set.WeightKg, Is.EqualTo(5.0));
    }

    [Test]
    public async Task WhenWorkoutHasMultipleExercisesWithMultipleSets_ShouldReturnAllData()
    {
        var exercise1Id = Guid.CreateVersion7();
        var exercise2Id = Guid.CreateVersion7();
        var workoutId = Guid.CreateVersion7();
        var workoutDate = DateTime.UtcNow.AddHours(-1);

        await SeedExercise(exercise1Id, "Pushups");
        await SeedExercise(exercise2Id, "Squats");
        await SeedWorkoutWithMultipleExercisesAndSets(workoutId, exercise1Id, exercise2Id, workoutDate);

        var response = await GetWorkout(workoutId.ToString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var workout = await ParseWorkoutResponse(response);
        Assert.That(workout, Is.Not.Null);
        Assert.That(workout!.Id, Is.EqualTo(workoutId));
        Assert.That(workout.WorkoutActivities.Count(), Is.EqualTo(2)); // 2 exercises

        // Verify Pushups (2 sets)
        var pushups = workout.WorkoutActivities.FirstOrDefault(a => a.ExerciseName == "Pushups");
        Assert.That(pushups, Is.Not.Null);
        Assert.That(pushups!.Sets.Count(), Is.EqualTo(2));
        var pushupSets = pushups.Sets.ToList();
        Assert.That(pushupSets, Has.One.Matches<Sets>(s => s.Repetitions == 10 && s.WeightKg == 5.0));
        Assert.That(pushupSets, Has.One.Matches<Sets>(s => s.Repetitions == 8 && s.WeightKg == 7.5));

        // Verify Squats (3 sets)
        var squats = workout.WorkoutActivities.FirstOrDefault(a => a.ExerciseName == "Squats");
        Assert.That(squats, Is.Not.Null);
        Assert.That(squats!.Sets.Count(), Is.EqualTo(3));
        var squatSets = squats.Sets.ToList();
        Assert.That(squatSets, Has.One.Matches<Sets>(s => s.Repetitions == 12 && s.WeightKg == 20.0));
        Assert.That(squatSets, Has.One.Matches<Sets>(s => s.Repetitions == 10 && s.WeightKg == 25.0));
        Assert.That(squatSets, Has.One.Matches<Sets>(s => s.Repetitions == 8 && s.WeightKg == 30.0));
    }

    [Test]
    public async Task WhenWorkoutHasNullNotes_ShouldReturnWithoutNotes()
    {
        var exerciseId = Guid.CreateVersion7();
        var workoutId = Guid.CreateVersion7();

        await SeedExercise(exerciseId, "Bench Press");
        await SeedWorkoutWithNullNotes(workoutId, exerciseId);

        var response = await GetWorkout(workoutId.ToString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var workout = await ParseWorkoutResponse(response);
        Assert.That(workout, Is.Not.Null);
        Assert.That(workout!.Notes, Is.Null);
        Assert.That(workout.Id, Is.EqualTo(workoutId));
        Assert.That(workout.WorkoutActivities.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task WhenExerciseHasZeroWeight_ShouldReturnZeroWeight()
    {
        var exerciseId = Guid.CreateVersion7();
        var workoutId = Guid.CreateVersion7();

        await SeedExercise(exerciseId, "Pushups");
        await SeedWorkoutWithZeroWeight(workoutId, exerciseId);

        var response = await GetWorkout(workoutId.ToString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var workout = await ParseWorkoutResponse(response);
        Assert.That(workout, Is.Not.Null);
        var activity = workout!.WorkoutActivities.First();
        var set = activity.Sets.First();
        Assert.That(set.WeightKg, Is.EqualTo(0.0));
        Assert.That(set.Repetitions, Is.EqualTo(15));
    }

    #endregion

    #region Helper Methods

    private async Task<HttpResponseMessage> GetWorkout(string workoutId)
    {
        return await client.GetAsync($"/workouts/{workoutId}");
    }

    private async Task<WorkoutResponse?> ParseWorkoutResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<WorkoutResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private async Task SeedExercise(Guid exerciseId, string name)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();
        db.Exercises.Add(ExerciseTableBuilder.Create().WithId(exerciseId).WithName(name).Build());
        await db.SaveChangesAsync();
    }

    private async Task SeedWorkoutWithSingleExercise(Guid workoutId, Guid exerciseId, DateTimeOffset workoutDate)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        var workout = new Infrastructure.Database.Tables.Workout
        {
            Id = workoutId,
            Notes = "Single Exercise Workout",
            TotalDurationMinutes = 30,
            WorkoutDate = workoutDate
        };
        db.Workouts.Add(workout);
        await db.SaveChangesAsync();

        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            ExerciseId = exerciseId,
            Repetitions = 10,
            WeightKg = 5.0
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedWorkoutWithMultipleExercisesAndSets(Guid workoutId, Guid exercise1Id, Guid exercise2Id, DateTime workoutDate)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        var workout = new Infrastructure.Database.Tables.Workout
        {
            Id = workoutId,
            Notes = "Multiple Exercises Workout",
            TotalDurationMinutes = 60,
            WorkoutDate = workoutDate
        };
        db.Workouts.Add(workout);
        await db.SaveChangesAsync();

        // Add 2 sets for Pushups
        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            ExerciseId = exercise1Id,
            Repetitions = 10,
            WeightKg = 5.0
        });
        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            ExerciseId = exercise1Id,
            Repetitions = 8,
            WeightKg = 7.5
        });

        // Add 3 sets for Squats
        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            ExerciseId = exercise2Id,
            Repetitions = 12,
            WeightKg = 20.0
        });
        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            ExerciseId = exercise2Id,
            Repetitions = 10,
            WeightKg = 25.0
        });
        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            ExerciseId = exercise2Id,
            Repetitions = 8,
            WeightKg = 30.0
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedWorkoutWithNullNotes(Guid workoutId, Guid exerciseId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        var workout = new Infrastructure.Database.Tables.Workout
        {
            Id = workoutId,
            Notes = null,
            TotalDurationMinutes = 45,
            WorkoutDate = DateTime.UtcNow.AddHours(-3)
        };
        db.Workouts.Add(workout);
        await db.SaveChangesAsync();

        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            ExerciseId = exerciseId,
            Repetitions = 12,
            WeightKg = 10.0
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedWorkoutWithZeroWeight(Guid workoutId, Guid exerciseId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        var workout = new Infrastructure.Database.Tables.Workout
        {
            Id = workoutId,
            Notes = "Bodyweight Exercise",
            TotalDurationMinutes = 20,
            WorkoutDate = DateTime.UtcNow.AddHours(-4)
        };
        db.Workouts.Add(workout);
        await db.SaveChangesAsync();

        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            ExerciseId = exerciseId,
            Repetitions = 15,
            WeightKg = 0.0
        });
        await db.SaveChangesAsync();
    }

    #endregion
}
