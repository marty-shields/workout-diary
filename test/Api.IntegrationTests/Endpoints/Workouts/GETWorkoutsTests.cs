using Api.IntegrationTests.Builders;
using Api.Models.Workouts;
using Core.Queries;
using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Api.IntegrationTests.Endpoints.Workouts;

public class GETWorkoutsTests : BaseTestFixture
{
    #region Validation Tests - Authentication

    [Test]
    public async Task WhenTokenIsMissing_ShouldReturnUnauthorized()
    {
        var response = await client.GetAsync("/api/workouts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task WhenSubjectClaimIsMissing_ShouldReturnUnauthorized()
    {
        // Build a token without the subject claim
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            JwtTokenBuilder.Create().Build());

        var response = await client.GetAsync("/api/workouts");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    #endregion

    #region Validation Tests - User Isolation

    [Test]
    public async Task WhenWorkoutsExistForAnotherUser_ShouldReturnEmptyList()
    {
        var exercise = seededExercises.First();
        var workoutId = Guid.CreateVersion7();
        var workoutDate = DateTimeOffset.UtcNow.AddHours(-2);
        string ownerUserId = Guid.NewGuid().ToString();
        string requestingUserId = Guid.NewGuid().ToString();

        await SeedWorkoutWithSingleExercise(workoutId, ownerUserId, exercise.Id, workoutDate);

        var response = await GetWorkouts(requestingUserId);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var paginatedResult = await ParseWorkoutsResponse(response);
        Assert.That(paginatedResult, Is.Not.Null);
        Assert.That(paginatedResult.Items, Is.Empty);
    }

    #endregion

    #region Happy Path Tests

    [Test]
    public async Task WhenNoWorkoutsExist_ShouldReturnEmptyList()
    {
        string userId = Guid.NewGuid().ToString();
        var response = await GetWorkouts(userId);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var paginatedResult = await ParseWorkoutsResponse(response);
        Assert.That(paginatedResult, Is.Not.Null);
        Assert.That(paginatedResult.Items, Is.Empty);
        Assert.That(paginatedResult.TotalRecords, Is.EqualTo(0));
        Assert.That(paginatedResult.TotalPages, Is.EqualTo(0));
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

        var paginatedResult = await ParseWorkoutsResponse(response);
        Assert.That(paginatedResult, Is.Not.Null);
        Assert.That(paginatedResult.Items.Count(), Is.EqualTo(1));
        Assert.That(paginatedResult.TotalRecords, Is.EqualTo(1));
        Assert.That(paginatedResult.TotalPages, Is.EqualTo(1));

        var workout = paginatedResult.Items.First();
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

        var paginatedResult = await ParseWorkoutsResponse(response);
        Assert.That(paginatedResult, Is.Not.Null);
        Assert.That(paginatedResult.Items.Count(), Is.EqualTo(3));

        // Should be ordered by date descending (newest first)
        var workoutIds = paginatedResult.Items.Select(w => w.Id).ToList();
        Assert.That(workoutIds[0], Is.EqualTo(workout3Id));
        Assert.That(workoutIds[1], Is.EqualTo(workout2Id));
        Assert.That(workoutIds[2], Is.EqualTo(workout1Id));

        // Verify dates are in descending order
        var workoutDates = paginatedResult.Items.Select(w => w.WorkoutDate).ToList();
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

        var paginatedResult = await ParseWorkoutsResponse(response);
        Assert.That(paginatedResult, Is.Not.Null);
        Assert.That(paginatedResult.Items.Count(), Is.EqualTo(1));

        var workout = paginatedResult.Items.First();
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

    #region Pagination Tests

    [Test]
    public async Task WhenPageSizeIsNotProvided_ShouldUseDefaultPageSize()
    {
        var exerciseId = seededExercises.First().Id;
        string userId = Guid.NewGuid().ToString();

        // Create 50 workouts
        for (int i = 0; i < 50; i++)
        {
            await SeedWorkoutWithSingleExercise(
                Guid.CreateVersion7(),
                userId,
                exerciseId,
                DateTime.UtcNow.AddHours(-i),
                $"Workout {i}");
        }

        var response = await GetWorkouts(userId, pageSize: null, pageNumber: 1);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var paginatedResult = await ParseWorkoutsResponse(response);
        Assert.That(paginatedResult, Is.Not.Null);
        Assert.That(paginatedResult.Items.Count(), Is.EqualTo(50)); // Default page size is 100, so all 50 fit
    }

    [Test]
    public async Task WhenPageNumberIsNotProvided_ShouldUseDefaultPageNumber()
    {
        var exerciseId = seededExercises.First().Id;
        string userId = Guid.NewGuid().ToString();

        // Create 10 workouts
        for (int i = 0; i < 10; i++)
        {
            await SeedWorkoutWithSingleExercise(
                Guid.CreateVersion7(),
                userId,
                exerciseId,
                DateTime.UtcNow.AddHours(-i),
                $"Workout {i}");
        }

        var response = await GetWorkouts(userId, pageSize: 5, pageNumber: null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var paginatedResult = await ParseWorkoutsResponse(response);
        Assert.That(paginatedResult, Is.Not.Null);
        Assert.That(paginatedResult.Items.Count(), Is.EqualTo(5)); // First page with page size 5
    }

    [Test]
    public async Task WhenPageSizeExceedsMaximum_ShouldReturnBadRequest()
    {
        string userId = Guid.NewGuid().ToString();
        var response = await GetWorkouts(userId, pageSize: 101, pageNumber: 1);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task WhenPageSizeIsZero_ShouldReturnBadRequest()
    {
        string userId = Guid.NewGuid().ToString();
        var response = await GetWorkouts(userId, pageSize: 0, pageNumber: 1);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task WhenPageNumberIsZero_ShouldReturnBadRequest()
    {
        string userId = Guid.NewGuid().ToString();
        var response = await GetWorkouts(userId, pageSize: 10, pageNumber: 0);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task WhenPageNumberIsNegative_ShouldReturnBadRequest()
    {
        string userId = Guid.NewGuid().ToString();
        var response = await GetWorkouts(userId, pageSize: 10, pageNumber: -1);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task WhenRequestingSecondPage_ShouldReturnCorrectWorkouts()
    {
        var exerciseId = seededExercises.First().Id;
        string userId = Guid.NewGuid().ToString();

        // Create 15 workouts
        var workoutIds = new List<Guid>();
        for (int i = 0; i < 15; i++)
        {
            var workoutId = Guid.CreateVersion7();
            workoutIds.Add(workoutId);
            await SeedWorkoutWithSingleExercise(
                workoutId,
                userId,
                exerciseId,
                DateTime.UtcNow.AddHours(-i),
                $"Workout {i}");
        }

        // Get first page (5 items)
        var response1 = await GetWorkouts(userId, pageSize: 5, pageNumber: 1);
        var paginatedResult1 = await ParseWorkoutsResponse(response1);
        Assert.That(paginatedResult1, Is.Not.Null);
        Assert.That(paginatedResult1.Items.Count(), Is.EqualTo(5));

        // Get second page (5 items)
        var response2 = await GetWorkouts(userId, pageSize: 5, pageNumber: 2);
        var paginatedResult2 = await ParseWorkoutsResponse(response2);
        Assert.That(paginatedResult2, Is.Not.Null);
        Assert.That(paginatedResult2.Items.Count(), Is.EqualTo(5));

        // Get third page (5 items)
        var response3 = await GetWorkouts(userId, pageSize: 5, pageNumber: 3);
        var paginatedResult3 = await ParseWorkoutsResponse(response3);
        Assert.That(paginatedResult3, Is.Not.Null);
        Assert.That(paginatedResult3.Items.Count(), Is.EqualTo(5));

        // Verify no duplicates between pages
        var page1Ids = paginatedResult1.Items.Select(w => w.Id).ToList();
        var page2Ids = paginatedResult2.Items.Select(w => w.Id).ToList();
        var page3Ids = paginatedResult3.Items.Select(w => w.Id).ToList();

        Assert.That(page1Ids.Intersect(page2Ids), Is.Empty);
        Assert.That(page2Ids.Intersect(page3Ids), Is.Empty);
        Assert.That(page1Ids.Intersect(page3Ids), Is.Empty);
    }

    [Test]
    public async Task WhenPageNumberExceedsAvailablePages_ShouldReturnEmptyList()
    {
        var exerciseId = seededExercises.First().Id;
        string userId = Guid.NewGuid().ToString();

        // Create 5 workouts
        for (int i = 0; i < 5; i++)
        {
            await SeedWorkoutWithSingleExercise(
                Guid.CreateVersion7(),
                userId,
                exerciseId,
                DateTime.UtcNow.AddHours(-i),
                $"Workout {i}");
        }

        // Request page 10 with page size 5 (only 1 page exists)
        var response = await GetWorkouts(userId, pageSize: 5, pageNumber: 10);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var paginatedResult = await ParseWorkoutsResponse(response);
        Assert.That(paginatedResult, Is.Not.Null);
        Assert.That(paginatedResult.Items, Is.Empty);
    }

    [Test]
    public async Task WhenUsingDifferentPageSizes_ShouldReturnCorrectCounts()
    {
        var exerciseId = seededExercises.First().Id;
        string userId = Guid.NewGuid().ToString();

        // Create 20 workouts
        for (int i = 0; i < 20; i++)
        {
            await SeedWorkoutWithSingleExercise(
                Guid.CreateVersion7(),
                userId,
                exerciseId,
                DateTime.UtcNow.AddHours(-i),
                $"Workout {i}");
        }

        // Test different page sizes
        var response10 = await GetWorkouts(userId, pageSize: 10, pageNumber: 1);
        var paginatedResult10 = await ParseWorkoutsResponse(response10);
        Assert.That(paginatedResult10, Is.Not.Null);
        Assert.That(paginatedResult10.Items.Count(), Is.EqualTo(10));

        var response7 = await GetWorkouts(userId, pageSize: 7, pageNumber: 1);
        var paginatedResult7 = await ParseWorkoutsResponse(response7);
        Assert.That(paginatedResult7, Is.Not.Null);
        Assert.That(paginatedResult7.Items.Count(), Is.EqualTo(7));

        var response20 = await GetWorkouts(userId, pageSize: 20, pageNumber: 1);
        var paginatedResult20 = await ParseWorkoutsResponse(response20);
        Assert.That(paginatedResult20, Is.Not.Null);
        Assert.That(paginatedResult20.Items.Count(), Is.EqualTo(20));
    }

    [Test]
    public async Task WhenPaginatingOrderByDateDescendingIsMaintained()
    {
        var exerciseId = seededExercises.First().Id;
        string userId = Guid.NewGuid().ToString();

        // Create 10 workouts in specific order
        for (int i = 0; i < 10; i++)
        {
            await SeedWorkoutWithSingleExercise(
                Guid.CreateVersion7(),
                userId,
                exerciseId,
                DateTime.UtcNow.AddHours(-i),
                $"Workout {i}");
        }

        // Get all workouts in pages of 3
        var page1 = await GetWorkouts(userId, pageSize: 3, pageNumber: 1);
        var paginatedResult1 = await ParseWorkoutsResponse(page1);

        var page2 = await GetWorkouts(userId, pageSize: 3, pageNumber: 2);
        var paginatedResult2 = await ParseWorkoutsResponse(page2);

        var page3 = await GetWorkouts(userId, pageSize: 3, pageNumber: 3);
        var paginatedResult3 = await ParseWorkoutsResponse(page3);

        var page4 = await GetWorkouts(userId, pageSize: 3, pageNumber: 4);
        var paginatedResult4 = await ParseWorkoutsResponse(page4);

        // Combine all pages and verify they're in descending date order
        var allWorkouts = paginatedResult1!.Items.Concat(paginatedResult2!.Items)
            .Concat(paginatedResult3!.Items).Concat(paginatedResult4!.Items).ToList();

        var dates = allWorkouts.Select(w => w.WorkoutDate).ToList();
        for (int i = 0; i < dates.Count - 1; i++)
        {
            Assert.That(dates[i], Is.GreaterThanOrEqualTo(dates[i + 1]),
                "Workouts should be ordered by date descending across all pages");
        }
    }

    #endregion

    #region Helper Methods

    private async Task<HttpResponseMessage> GetWorkouts(string userId, int? pageSize = null, int? pageNumber = null)
    {
        AddJWTTokenToRequest(userId);
        var queryParams = new List<string>();
        if (pageSize.HasValue) queryParams.Add($"pageSize={pageSize.Value}");
        if (pageNumber.HasValue) queryParams.Add($"pageNumber={pageNumber.Value}");

        var url = "/api/workouts" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
        return await client.GetAsync(url);
    }

    private async Task<PaginatedResult<WorkoutResponse>?> ParseWorkoutsResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaginatedResult<WorkoutResponse>>(content, new JsonSerializerOptions
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