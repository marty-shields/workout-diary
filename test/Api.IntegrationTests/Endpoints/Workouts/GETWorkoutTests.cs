using Api.IntegrationTests.Builders;
using Api.Models.Workouts;
using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Api.IntegrationTests.Endpoints.Workouts;

public class GETWorkoutTests : BaseTestFixture
{
    #region Validation Tests - Authentication

    [Test]
    public async Task WhenTokenIsMissing_ShouldReturnUnauthorized()
    {
        string userId = Guid.NewGuid().ToString();
        var workoutId = Guid.CreateVersion7();
        var response = await client.GetAsync($"/api/workouts/{workoutId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task WhenSubjectClaimIsMissing_ShouldReturnUnauthorized()
    {
        string userId = Guid.NewGuid().ToString();
        var workoutId = Guid.CreateVersion7();

        // Build a token without the subject claim
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            JwtTokenBuilder.Create().Build());

        var response = await client.GetAsync($"/api/workouts/{workoutId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    #endregion

    #region Validation Tests - Invalid ID Format

    [Test]
    public async Task WhenWorkoutIdIsInvalidFormat_ShouldReturnNotFound()
    {
        string userId = Guid.NewGuid().ToString();
        var response = await GetWorkout(userId, "invalid-guid");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region Validation Tests - Workout Not Found

    [Test]
    public async Task WhenWorkoutDoesNotExist_ShouldReturnNotFound()
    {
        string userId = Guid.NewGuid().ToString();
        var nonExistentId = Guid.CreateVersion7();
        var response = await GetWorkout(userId, nonExistentId.ToString());

        Assert.That(response.StatusCode, Is.AnyOf(HttpStatusCode.NotFound));
    }

    #endregion

    #region Validation Tests - User Isolation

    [Test]
    public async Task WhenWorkoutBelongsToAnotherUser_ShouldReturnNotFound()
    {
        var exercise = seededExercises.First();
        var workoutId = Guid.CreateVersion7();
        var workoutDate = DateTimeOffset.UtcNow.AddHours(-2);
        string ownerUserId = Guid.NewGuid().ToString();
        string requestingUserId = Guid.NewGuid().ToString();

        await SeedWorkoutWithSingleExercise(workoutId, ownerUserId, exercise.Id, workoutDate);

        var response = await GetWorkout(requestingUserId, workoutId.ToString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region Happy Path Tests

    [Test]
    public async Task WhenWorkoutExists_ShouldReturnOkWithWorkoutData()
    {
        var exercise = seededExercises.First();
        var workoutId = Guid.CreateVersion7();
        var workoutDate = DateTimeOffset.UtcNow.AddHours(-2);
        string userId = Guid.NewGuid().ToString();

        await SeedWorkoutWithSingleExercise(workoutId, userId, exercise.Id, workoutDate);

        var response = await GetWorkout(userId, workoutId.ToString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected OK but got {response.StatusCode}. Response: {await response.Content.ReadAsStringAsync()}");

        var workout = await ParseWorkoutResponse(response);
        Assert.That(workout, Is.Not.Null);
        Assert.That(workout!.Id, Is.EqualTo(workoutId));
        Assert.That(workout.Notes, Is.EqualTo("Single Exercise Workout"));
        Assert.That(workout.TotalDurationMinutes, Is.EqualTo(30));
        Assert.That(workout.WorkoutDate.ToString(), Is.EqualTo(workoutDate.ToString()));
        Assert.That(workout.WorkoutActivities.Count(), Is.EqualTo(1));
        var activity = workout.WorkoutActivities.First();
        Assert.That(activity.ExerciseName, Is.EqualTo(exercise.Name));
        Assert.That(activity.Sets.Count(), Is.EqualTo(1));
        var set = activity.Sets.First();
        Assert.That(set.Repetitions, Is.EqualTo(10));
        Assert.That(set.WeightKg, Is.EqualTo(5.0));
    }

    [Test]
    public async Task WhenWorkoutHasMultipleExercisesWithMultipleSets_ShouldReturnAllData()
    {
        var exercise1 = seededExercises.First();
        var exercise2 = seededExercises.Last();
        var workoutId = Guid.CreateVersion7();
        var workoutDate = DateTime.UtcNow.AddHours(-1);
        string userId = Guid.NewGuid().ToString();

        await SeedWorkoutWithMultipleExercisesAndSets(workoutId, userId, exercise1.Id, exercise2.Id, workoutDate);

        var response = await GetWorkout(userId, workoutId.ToString());
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var workout = await ParseWorkoutResponse(response);
        Assert.That(workout, Is.Not.Null);
        Assert.That(workout!.Id, Is.EqualTo(workoutId));
        Assert.That(workout.WorkoutActivities.Count(), Is.EqualTo(2)); // 2 exercises

        // Verify Pushups (2 sets)
        var activity1 = workout.WorkoutActivities.FirstOrDefault(a => a.ExerciseName == exercise1.Name);
        Assert.That(activity1, Is.Not.Null);
        Assert.That(activity1!.Sets.Count(), Is.EqualTo(2));
        var activity1Sets = activity1.Sets.ToList();
        Assert.That(activity1Sets, Has.One.Matches<Sets>(s => s.Repetitions == 10 && s.WeightKg == 5.0));
        Assert.That(activity1Sets, Has.One.Matches<Sets>(s => s.Repetitions == 8 && s.WeightKg == 7.5));

        // Verify Squats (3 sets)
        var activity2 = workout.WorkoutActivities.FirstOrDefault(a => a.ExerciseName == exercise2.Name);
        Assert.That(activity2, Is.Not.Null);
        Assert.That(activity2!.Sets.Count(), Is.EqualTo(3));
        var activity2Sets = activity2.Sets.ToList();
        Assert.That(activity2Sets, Has.One.Matches<Sets>(s => s.Repetitions == 12 && s.WeightKg == 20.0));
        Assert.That(activity2Sets, Has.One.Matches<Sets>(s => s.Repetitions == 10 && s.WeightKg == 25.0));
        Assert.That(activity2Sets, Has.One.Matches<Sets>(s => s.Repetitions == 8 && s.WeightKg == 30.0));
    }

    [Test]
    public async Task WhenWorkoutHasNullNotes_ShouldReturnWithoutNotes()
    {
        var exercise = seededExercises.First();
        var workoutId = Guid.CreateVersion7();
        string userId = Guid.NewGuid().ToString();

        await SeedWorkoutWithNullNotes(workoutId, userId, exercise.Id);

        var response = await GetWorkout(userId, workoutId.ToString());
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
        var exercise = seededExercises.First();
        var workoutId = Guid.CreateVersion7();
        string userId = Guid.NewGuid().ToString();

        await SeedWorkoutWithZeroWeight(workoutId, userId, exercise.Id);

        var response = await GetWorkout(userId, workoutId.ToString());
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

    private async Task<HttpResponseMessage> GetWorkout(string userId, string workoutId)
    {
        AddJWTTokenToRequest(userId);
        return await client.GetAsync($"/api/workouts/{workoutId}");
    }

    private async Task<WorkoutResponse?> ParseWorkoutResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<WorkoutResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private async Task SeedWorkoutWithSingleExercise(Guid workoutId, string userId, Guid exerciseId, DateTimeOffset workoutDate)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        var workout = new Infrastructure.Database.Tables.Workout
        {
            Id = workoutId,
            UserId = userId,
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
            WorkoutUserId = userId,
            ExerciseId = exerciseId,
            Repetitions = 10,
            WeightKg = 5.0
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedWorkoutWithMultipleExercisesAndSets(Guid workoutId, string userId, Guid exercise1Id, Guid exercise2Id, DateTime workoutDate)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        var workout = new Infrastructure.Database.Tables.Workout
        {
            Id = workoutId,
            UserId = userId,
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
            WorkoutUserId = userId,
            ExerciseId = exercise1Id,
            Repetitions = 10,
            WeightKg = 5.0
        });
        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            WorkoutUserId = userId,
            ExerciseId = exercise1Id,
            Repetitions = 8,
            WeightKg = 7.5
        });

        // Add 3 sets for Squats
        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            WorkoutUserId = userId,
            ExerciseId = exercise2Id,
            Repetitions = 12,
            WeightKg = 20.0
        });
        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            WorkoutUserId = userId,
            ExerciseId = exercise2Id,
            Repetitions = 10,
            WeightKg = 25.0
        });
        db.WorkoutActivities.Add(new Infrastructure.Database.Tables.WorkoutActivity
        {
            Id = Guid.CreateVersion7(),
            WorkoutId = workoutId,
            WorkoutUserId = userId,
            ExerciseId = exercise2Id,
            Repetitions = 8,
            WeightKg = 30.0
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedWorkoutWithNullNotes(Guid workoutId, string userId, Guid exerciseId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        var workout = new Infrastructure.Database.Tables.Workout
        {
            Id = workoutId,
            UserId = userId,
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
            WorkoutUserId = userId,
            ExerciseId = exerciseId,
            Repetitions = 12,
            WeightKg = 10.0
        });
        await db.SaveChangesAsync();
    }

    private async Task SeedWorkoutWithZeroWeight(Guid workoutId, string userId, Guid exerciseId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        var workout = new Infrastructure.Database.Tables.Workout
        {
            Id = workoutId,
            UserId = userId,
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
            WorkoutUserId = userId,
            ExerciseId = exerciseId,
            Repetitions = 15,
            WeightKg = 0.0
        });
        await db.SaveChangesAsync();
    }

    #endregion
}
