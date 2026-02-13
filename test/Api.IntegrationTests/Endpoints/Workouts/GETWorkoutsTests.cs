using Api.IntegrationTests.Builders;
using Api.Models.Workouts;
using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Api.IntegrationTests.Endpoints.Workouts;

public class GETWorkoutsTests : BaseTestFixture
{
    #region Happy Path Tests

    [Test]
    public async Task WhenNoWorkoutsExist_ShouldReturnEmptyList()
    {
        string userId = Guid.NewGuid().ToString();
        var response = await GetWorkouts(userId);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var workouts = await ParseWorkoutsResponse(response);
        Assert.That(workouts, Is.Not.Null);
        Assert.That(workouts, Is.Empty);
    }

    [Test]
    public async Task WhenSingleWorkoutExists_ShouldReturnListWithOneWorkout()
    {
        var exercise = seededExercises.First();
        var workoutId = Guid.CreateVersion7();
        var workoutDate = DateTimeOffset.UtcNow.AddHours(-2);
        string userId = Guid.NewGuid().ToString();

        await SeedWorkoutWithSingleExercise(workoutId, userId, exercise.Id, workoutDate);

        var response = await GetWorkouts(userId);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var workouts = await ParseWorkoutsResponse(response);
        Assert.That(workouts, Is.Not.Null);
        Assert.That(workouts.Count(), Is.EqualTo(1));

        var workout = workouts.First();
        Assert.That(workout.Id, Is.EqualTo(workoutId));
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
    public async Task WhenMultipleWorkoutsExist_ShouldReturnAllWorkoutsOrderedByDateDescending()
    {
        var exerciseId = seededExercises.First().Id;
        var workout1Id = Guid.CreateVersion7();
        var workout2Id = Guid.CreateVersion7();
        var workout3Id = Guid.CreateVersion7();
        var workout1Date = DateTime.UtcNow.AddHours(-3);
        var workout2Date = DateTime.UtcNow.AddHours(-2);
        var workout3Date = DateTime.UtcNow.AddHours(-1);
        string userId = Guid.NewGuid().ToString();

        // Create workouts in non-chronological order to test ordering
        await SeedWorkoutWithSingleExercise(workout2Id, userId, exerciseId, workout2Date, "Workout 2");
        await SeedWorkoutWithSingleExercise(workout1Id, userId, exerciseId, workout1Date, "Workout 1");
        await SeedWorkoutWithSingleExercise(workout3Id, userId, exerciseId, workout3Date, "Workout 3");

        var response = await GetWorkouts(userId);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var workouts = await ParseWorkoutsResponse(response);
        Assert.That(workouts, Is.Not.Null);
        Assert.That(workouts.Count(), Is.EqualTo(3));

        // Should be ordered by date descending (newest first)
        var workoutIds = workouts.Select(w => w.Id).ToList();
        Assert.That(workoutIds[0], Is.EqualTo(workout3Id));
        Assert.That(workoutIds[1], Is.EqualTo(workout2Id));
        Assert.That(workoutIds[2], Is.EqualTo(workout1Id));

        // Verify dates are in descending order
        var workoutDates = workouts.Select(w => w.WorkoutDate).ToList();
        Assert.That(workoutDates[0], Is.GreaterThan(workoutDates[1]));
        Assert.That(workoutDates[1], Is.GreaterThan(workoutDates[2]));
    }

    [Test]
    public async Task WhenWorkoutsHaveDifferentExercises_ShouldReturnAllExerciseData()
    {
        var exercise1 = seededExercises.First();
        var exercise2 = seededExercises.Last();
        var squatsId = Guid.CreateVersion7();
        var workoutId = Guid.CreateVersion7();
        var workoutDate = DateTime.UtcNow.AddHours(-1);
        string userId = Guid.NewGuid().ToString();

        await SeedWorkoutWithMultipleExercisesAndSets(workoutId, userId, exercise1.Id, exercise2.Id, workoutDate);

        var response = await GetWorkouts(userId);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var workouts = await ParseWorkoutsResponse(response);
        Assert.That(workouts, Is.Not.Null);
        Assert.That(workouts.Count(), Is.EqualTo(1));

        var workout = workouts.First();
        Assert.That(workout.WorkoutActivities.Count(), Is.EqualTo(2));

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

    #endregion

    #region Helper Methods

    private async Task<HttpResponseMessage> GetWorkouts(string userId)
    {
        AddJWTTokenToRequest(userId);
        return await client.GetAsync("/api/workouts");
    }

    private async Task<IEnumerable<WorkoutResponse>?> ParseWorkoutsResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<WorkoutResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private async Task SeedWorkoutWithSingleExercise(Guid workoutId, string userId, Guid exerciseId, DateTimeOffset workoutDate, string notes = "Single Exercise Workout")
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        var workout = new Infrastructure.Database.Tables.Workout
        {
            Id = workoutId,
            UserId = userId,
            Notes = notes,
            TotalDurationMinutes = 30,
            WorkoutDate = workoutDate
        };
        db.Workouts.Add(workout);
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

    #endregion
}