using Api.IntegrationTests.Builders;
using Api.Models.Workouts;
using Infrastructure.Database;
using Infrastructure.Database.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Api.IntegrationTests.Endpoints.Workouts;

public class POSTWorkoutTests : BaseTestFixture
{
    private const string DefaultNotes = "Test";
    private const int DefaultDuration = 30;
    private static readonly DateTime DefaultWorkoutDate = DateTime.UtcNow.AddHours(-1);

    #region Validation Tests - Request Properties

    [Test]
    public async Task WhenExercisesPropertyIsMissing_ShouldReturnBadRequest()
    {
        var request = new { notes = DefaultNotes, totalDurationMinutes = DefaultDuration, workoutDate = DefaultWorkoutDate };
        var response = await PostWorkoutAnon(request);
        await AssertValidationError(response, "$", "JSON deserialization for type 'Api.Models.Workouts.WorkoutRequest' was missing required properties including: 'exercises'.");
    }

    [TestCase(null, "Exercises", "The Exercises field is required.")]
    [TestCase("empty", "Exercises", "The field Exercises must be a string or array type with a minimum length of '1'.")]
    public async Task WhenExercisesIsInvalid_ShouldReturnBadRequest(string? exercisesCase, string fieldName, string expectedError)
    {
        HttpResponseMessage response;
        if (exercisesCase == null)
        {
            var request = new { exercises = (Exercise[]?)null, notes = DefaultNotes, totalDurationMinutes = DefaultDuration, workoutDate = DefaultWorkoutDate };
            response = await PostWorkoutAnon(request);
        }
        else
        {
            var request = WorkoutRequestBuilder.Create().WithEmptyExercises().Build();
            response = await PostWorkout(request);
        }
        await AssertValidationError(response, fieldName, expectedError);
    }

    [TestCase(0, "TotalDurationMinutes", "The field TotalDurationMinutes must be between 1 and 2147483647.")]
    [TestCase(-5, "TotalDurationMinutes", "The field TotalDurationMinutes must be between 1 and 2147483647.")]
    public async Task WhenTotalDurationMinutesIsInvalid_ShouldReturnBadRequest(int duration, string fieldName, string expectedError)
    {
        var request = WorkoutRequestBuilder.Create()
            .WithExercises(ExerciseBuilder.Create().Build())
            .WithDuration(duration)
            .Build();

        var response = await PostWorkout(request);
        await AssertValidationError(response, fieldName, expectedError);
    }

    [Test]
    public async Task WhenTotalDurationMinutesPropertyIsMissing_ShouldReturnBadRequest()
    {
        var exercises = new[] { new { id = Guid.CreateVersion7(), sets = new[] { new { repetitions = 10, weightKg = 0.0 } } } };
        var request = new { exercises, notes = DefaultNotes, workoutDate = DefaultWorkoutDate };
        var response = await PostWorkoutAnon(request);
        await AssertValidationError(response, "$", "JSON deserialization for type 'Api.Models.Workouts.WorkoutRequest' was missing required properties including: 'totalDurationMinutes'.");
    }

    [Test]
    public async Task WhenWorkoutDatePropertyIsMissing_ShouldReturnBadRequest()
    {
        var exercises = new[] { new { id = Guid.CreateVersion7(), sets = new[] { new { repetitions = 10, weightKg = 0.0 } } } };
        var request = new { exercises, notes = DefaultNotes, totalDurationMinutes = DefaultDuration };
        var response = await PostWorkoutAnon(request);
        await AssertValidationError(response, "$", "JSON deserialization for type 'Api.Models.Workouts.WorkoutRequest' was missing required properties including: 'workoutDate'.");
    }

    [Test]
    public async Task WhenWorkoutDateIsInTheFuture_ShouldReturnBadRequest()
    {
        var futureDate = DateTime.UtcNow.AddHours(1);
        var request = WorkoutRequestBuilder.Create()
            .WithExercises(ExerciseBuilder.Create().Build())
            .WithWorkoutDate(futureDate)
            .Build();

        var response = await PostWorkout(request);
        await AssertValidationError(response, "WorkoutDate", "Workout date cannot be in the future.");
    }

    #endregion

    #region Validation Tests - Exercise Properties

    [Test]
    public async Task WhenExerciseIdIsNull_ShouldReturnBadRequest()
    {
        var request = new { exercises = new[] { new { id = (Guid?)null, sets = new[] { new { repetitions = 10, weightKg = 0.0 } } } }, notes = DefaultNotes, totalDurationMinutes = DefaultDuration, workoutDate = DefaultWorkoutDate };
        var response = await PostWorkoutAnon(request);
        await AssertValidationError(response, "$.exercises[0].id", "The JSON value could not be converted to System.Guid.");
    }

    [Test]
    public async Task WhenExerciseSetsIsNull_ShouldReturnBadRequest()
    {
        var request = new { exercises = new[] { new { id = Guid.CreateVersion7(), sets = (WorkoutSet[]?)null } }, notes = DefaultNotes, totalDurationMinutes = DefaultDuration, workoutDate = DefaultWorkoutDate };
        var response = await PostWorkoutAnon(request);
        await AssertValidationError(response, "Exercises[0].Sets", "The Sets field is required.");
    }

    [Test]
    public async Task WhenExerciseSetsIsEmpty_ShouldReturnBadRequest()
    {
        var exercises = new[] { ExerciseBuilder.Create().WithEmptySets().Build() };
        var request = WorkoutRequestBuilder.Create().WithExercises(exercises).Build();
        var response = await PostWorkout(request);
        await AssertValidationError(response, "Exercises[0].Sets", "The field Sets must be a string or array type with a minimum length of '1'.");
    }

    [Test]
    public async Task WhenMultipleExercisesAndOneHasInvalidSets_ShouldReturnBadRequest()
    {
        var exercises = new[]
        {
            ExerciseBuilder.Create().Build(),
            ExerciseBuilder.Create().WithEmptySets().Build()
        };
        var request = WorkoutRequestBuilder.Create().WithExercises(exercises).Build();
        var response = await PostWorkout(request);
        await AssertValidationError(response, "Exercises[1].Sets", "The field Sets must be a string or array type with a minimum length of '1'.");
    }

    [Test]
    public async Task WhenMultipleExercisesAndOneHasNullId_ShouldReturnBadRequest()
    {
        var request = new { exercises = new[] { new { id = (Guid?)Guid.CreateVersion7(), sets = new[] { new { repetitions = 10, weightKg = 0.0 } } }, new { id = (Guid?)null, sets = new[] { new { repetitions = 10, weightKg = 0.0 } } } }, notes = DefaultNotes, totalDurationMinutes = DefaultDuration, workoutDate = DefaultWorkoutDate };
        var response = await PostWorkoutAnon(request);
        await AssertValidationError(response, "$.exercises[1].id", "The JSON value could not be converted to System.Guid.");
    }

    #endregion

    #region Validation Tests - Set Properties

    [Test]
    public async Task WhenSetRepetitionsIsNull_ShouldReturnBadRequest()
    {
        var set = new { repetitions = (int?)null, weightKg = 0.0 };
        var exercises = new[] { new { id = Guid.CreateVersion7(), sets = new[] { set } } };
        var request = new { exercises, notes = DefaultNotes, totalDurationMinutes = DefaultDuration, workoutDate = DefaultWorkoutDate };
        var response = await PostWorkoutAnon(request);
        await AssertValidationError(response, "$.exercises[0].sets[0].repetitions", "The JSON value could not be converted to System.Int32.");
    }

    [TestCase(0, "The field Repetitions must be between 1 and 2147483647.")]
    [TestCase(-5, "The field Repetitions must be between 1 and 2147483647.")]
    public async Task WhenSetRepetitionsIsInvalid_ShouldReturnBadRequest(int repetitions, string expectedError)
    {
        var set = WorkoutSetBuilder.Create().WithRepetitions(repetitions).Build();
        var exercises = new[] { ExerciseBuilder.Create().WithSets(set).Build() };
        var request = WorkoutRequestBuilder.Create().WithExercises(exercises).Build();
        var response = await PostWorkout(request);
        await AssertValidationError(response, "Exercises[0].Sets[0].Repetitions", expectedError);
    }

    [Test]
    public async Task WhenSetWeightKgIsNull_ShouldReturnBadRequest()
    {
        var set = new { repetitions = 10, weightKg = (double?)null };
        var exercises = new[] { new { id = Guid.CreateVersion7(), sets = new[] { set } } };
        var request = new { exercises, notes = DefaultNotes, totalDurationMinutes = DefaultDuration, workoutDate = DefaultWorkoutDate };
        var response = await PostWorkoutAnon(request);
        await AssertValidationError(response, "$.exercises[0].sets[0].weightKg", "The JSON value could not be converted to System.Double.");
    }

    [Test]
    public async Task WhenSetWeightKgIsNegative_ShouldReturnBadRequest()
    {
        var set = WorkoutSetBuilder.Create().WithWeightKg(-5.0).Build();
        var exercises = new[] { ExerciseBuilder.Create().WithSets(set).Build() };
        var request = WorkoutRequestBuilder.Create().WithExercises(exercises).Build();
        var response = await PostWorkout(request);
        await AssertValidationError(response, "Exercises[0].Sets[0].WeightKg", "The field WeightKg must be between 0 and 1.7976931348623157E+308.");
    }

    [Test]
    public async Task WhenMultipleSetsAndOneHasInvalidRepetitions_ShouldReturnBadRequest()
    {
        var validSet = WorkoutSetBuilder.Create().WithRepetitions(10).Build();
        var invalidSet = WorkoutSetBuilder.Create().WithRepetitions(0).Build();
        var exercises = new[] { ExerciseBuilder.Create().WithSets(validSet, invalidSet).Build() };
        var request = WorkoutRequestBuilder.Create().WithExercises(exercises).Build();
        var response = await PostWorkout(request);
        await AssertValidationError(response, "Exercises[0].Sets[1].Repetitions", "The field Repetitions must be between 1 and 2147483647.");
    }

    [Test]
    public async Task WhenMultipleSetsAndOneHasNullWeight_ShouldReturnBadRequest()
    {
        var request = new { exercises = new[] { new { id = Guid.CreateVersion7(), sets = new[] { new { repetitions = 10, weightKg = (double?)10.0 }, new { repetitions = 10, weightKg = (double?)null } } } }, notes = DefaultNotes, totalDurationMinutes = DefaultDuration, workoutDate = DefaultWorkoutDate };
        var response = await PostWorkoutAnon(request);
        await AssertValidationError(response, "$.exercises[0].sets[1].weightKg", "The JSON value could not be converted to System.Double.");
    }

    [Test]
    public async Task WhenMultipleExercisesWithMultipleSetsAndOneSetIsInvalid_ShouldReturnBadRequest()
    {
        var validSet1 = WorkoutSetBuilder.Create().WithRepetitions(10).Build();
        var validSet2 = WorkoutSetBuilder.Create().WithRepetitions(15).Build();
        var validSet3 = WorkoutSetBuilder.Create().WithRepetitions(20).Build();
        var validSet4 = WorkoutSetBuilder.Create().WithRepetitions(5).Build();
        var invalidSet = WorkoutSetBuilder.Create().WithRepetitions(0).Build();

        var exercises = new[]
        {
            ExerciseBuilder.Create().WithSets(validSet1, validSet2).Build(),
            ExerciseBuilder.Create().WithSets(validSet3, invalidSet).Build(),
            ExerciseBuilder.Create().WithSets(validSet4, validSet4).Build()
        };
        var request = WorkoutRequestBuilder.Create().WithExercises(exercises).WithDuration(60).Build();
        var response = await PostWorkout(request);
        await AssertValidationError(response, "Exercises[1].Sets[1].Repetitions", "The field Repetitions must be between 1 and 2147483647.");
    }

    #endregion

    #region Validation Tests - Database Validation

    #endregion

    #region Validation Tests - Database Validation

    [Test]
    public async Task WhenNoExercisesInDatabase_ShouldReturnBadRequest()
    {
        var exercises = new[] { ExerciseBuilder.Create().Build() };
        var request = WorkoutRequestBuilder.Create().WithExercises(exercises).Build();
        var response = await PostWorkout(request);
        await AssertValidationError(response, "Exercises", "Exercises with IDs " + exercises[0].Id + " not found.");
    }

    [Test]
    public async Task WhenExerciseDoesNotExistInDatabase_ShouldReturnBadRequest()
    {
        var pushupId = Guid.CreateVersion7();
        var squatsId = Guid.CreateVersion7();
        await SeedExercises((pushupId, "Pushups"), (squatsId, "Squats"));

        var missingId1 = Guid.CreateVersion7();
        var missingId2 = Guid.CreateVersion7();
        var exercises = new[]
        {
            ExerciseBuilder.Create().WithId(pushupId).Build(),
            ExerciseBuilder.Create().WithId(missingId1).Build(),
            ExerciseBuilder.Create().WithId(missingId2).Build()
        };
        var request = WorkoutRequestBuilder.Create().WithExercises(exercises).Build();
        var response = await PostWorkout(request);

        var responseDetails = await GetErrorResponse(response);
        Assert.That(responseDetails?.Errors, Contains.Key("Exercises"));
        Assert.That(responseDetails?.Errors["Exercises"], Does.Contain($"Exercises with IDs {missingId1}, {missingId2} not found."));
    }

    #endregion

    #region Happy Path Tests

    [Test]
    public async Task WhenSingleValidActivity_ShouldReturnCreated()
    {
        var pushupId = Guid.CreateVersion7();
        await SeedExercises((pushupId, "Pushups"));

        var exercises = new[] { ExerciseBuilder.Create().WithId(pushupId).Build() };
        var request = WorkoutRequestBuilder.Create().WithExercises(exercises).Build();

        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var workoutInDb = await GetWorkoutFromDatabase();
        Assert.That(workoutInDb, Is.Not.Null);
        Assert.That(workoutInDb!.WorkoutActivities.Count, Is.EqualTo(1));

        var activity = workoutInDb.WorkoutActivities.First();
        Assert.That(activity.Exercise.Id, Is.EqualTo(pushupId));
        Assert.That(activity.Repetitions, Is.EqualTo(10));
        Assert.That(activity.WeightKg, Is.EqualTo(5.0));
    }

    [Test]
    public async Task MultipleValidActivities_ShouldReturnCreated()
    {
        var pushupId = Guid.CreateVersion7();
        var squatId = Guid.CreateVersion7();
        var plankId = Guid.CreateVersion7();
        await SeedExercises((pushupId, "Pushups"), (squatId, "Squats"), (plankId, "Plank"));

        var set1 = WorkoutSetBuilder.Create().WithRepetitions(30).WithWeightKg(0.0).Build();
        var set2 = WorkoutSetBuilder.Create().WithRepetitions(20).WithWeightKg(0.0).Build();
        var set3 = WorkoutSetBuilder.Create().WithRepetitions(10).WithWeightKg(5.0).Build();
        var set4 = WorkoutSetBuilder.Create().WithRepetitions(10).WithWeightKg(5.0).Build();
        var set5 = WorkoutSetBuilder.Create().WithRepetitions(15).WithWeightKg(10.0).Build();

        var repeatedExercises = new[]
        {
            ExerciseBuilder.Create().WithId(plankId).WithSets(set1).Build(),
            ExerciseBuilder.Create().WithId(plankId).WithSets(set2, set3).Build()
        };

        var noneRepeatedExercises = new[]
        {
            ExerciseBuilder.Create().WithId(pushupId).WithSets(set4).Build(),
            ExerciseBuilder.Create().WithId(squatId).WithSets(set5).Build()
        };

        var exercises = noneRepeatedExercises.Concat(repeatedExercises).ToArray();
        var request = WorkoutRequestBuilder.Create()
            .WithExercises(exercises)
            .WithDuration(45)
            .Build();

        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var workoutInDb = await GetWorkoutFromDatabase();
        Assert.That(workoutInDb, Is.Not.Null);
        Assert.That(workoutInDb!.WorkoutActivities.Count, Is.EqualTo(5));

        AssertActivitiesMatch(workoutInDb, noneRepeatedExercises, repeatedExercises);
        AssertLocationHeader(response, workoutInDb);
        await AssertResponseBody(response, workoutInDb);
    }

    #endregion

    #region Helper Methods

    private async Task SeedExercises(params (Guid id, string name)[] exercisesToSeed)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();

        foreach (var (id, name) in exercisesToSeed)
        {
            db.Exercises.Add(ExerciseTableBuilder.Create().WithId(id).WithName(name).Build());
        }

        await db.SaveChangesAsync();
    }

    private async Task<Infrastructure.Database.Tables.Workout?> GetWorkoutFromDatabase()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();
        return await db.Workouts
            .Include(w => w.WorkoutActivities)
            .ThenInclude(wa => wa.Exercise)
            .FirstOrDefaultAsync();
    }

    private void AssertActivitiesMatch(Infrastructure.Database.Tables.Workout workout, Api.Models.Workouts.Exercise[] noneRepeatedExercises, Api.Models.Workouts.Exercise[] repeatedExercises)
    {
        foreach (var exercise in noneRepeatedExercises)
        {
            var activity = workout.WorkoutActivities.FirstOrDefault(wa => wa.Exercise.Id == exercise.Id);
            Assert.That(activity, Is.Not.Null, $"Activity for exercise ID {exercise.Id} should exist");
            Assert.That(activity!.Repetitions, Is.EqualTo(exercise.Sets!.First().Repetitions));
            Assert.That(activity.WeightKg, Is.EqualTo(exercise.Sets!.First().WeightKg));
        }

        foreach (var exercise in repeatedExercises)
        {
            foreach (var set in exercise.Sets!)
            {
                var activity = workout.WorkoutActivities.FirstOrDefault(wa =>
                    wa.Exercise.Id == exercise.Id &&
                    wa.Repetitions == set.Repetitions &&
                    wa.WeightKg == set.WeightKg);
                Assert.That(activity, Is.Not.Null, $"Activity for exercise ID {exercise.Id} should exist");
            }
        }
    }

    private void AssertLocationHeader(HttpResponseMessage response, Infrastructure.Database.Tables.Workout workout)
    {
        Assert.That(response.Headers.Location!.ToString(), Is.EqualTo($"/workouts/{workout.Id}"));
    }

    private async Task AssertResponseBody(HttpResponseMessage response, Infrastructure.Database.Tables.Workout dbWorkout)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        var createdWorkout = JsonSerializer.Deserialize<WorkoutResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var expectedWorkout = dbWorkout.ToEntity();
        var expectedWorkoutResponse = WorkoutResponse.FromEntity(expectedWorkout);
        Assert.That(createdWorkout!.Id, Is.EqualTo(expectedWorkoutResponse.Id));
        Assert.That(createdWorkout.Notes, Is.EqualTo(expectedWorkoutResponse.Notes));
        Assert.That(createdWorkout.TotalDurationMinutes, Is.EqualTo(expectedWorkoutResponse.TotalDurationMinutes));
        Assert.That(createdWorkout.WorkoutDate.ToString(), Is.EqualTo(expectedWorkoutResponse.WorkoutDate.ToString()));
        Assert.That(createdWorkout.WorkoutActivities.Count(), Is.EqualTo(expectedWorkoutResponse.WorkoutActivities.Count()));
        Assert.That(createdWorkout.WorkoutActivities.Select(wa => wa.ExerciseName),
            Is.EquivalentTo(expectedWorkoutResponse.WorkoutActivities.Select(wa => wa.ExerciseName)));
    }

    #endregion

    private async Task<HttpResponseMessage> PostWorkout(WorkoutRequest request)
    {
        AddJWTTokenToRequest(Guid.NewGuid().ToString());
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await client.PostAsync("/api/workouts", content);
    }

    private async Task<HttpResponseMessage> PostWorkoutAnon(object request)
    {
        AddJWTTokenToRequest(Guid.NewGuid().ToString());
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await client.PostAsync("/api/workouts", content);
    }

    private async Task<ValidationProblemDetails?> GetErrorResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ValidationProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private async Task AssertValidationError(HttpResponseMessage response, string fieldName, string expectedMessage)
    {
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var errors = await GetErrorResponse(response);
        Assert.That(errors?.Errors, Contains.Key(fieldName), $"Error field '{fieldName}' should exist");
        Assert.That(string.Join(" ", errors.Errors[fieldName]), Does.Contain(expectedMessage),
            $"Error message for '{fieldName}' should contain '{expectedMessage}'");
    }
}
