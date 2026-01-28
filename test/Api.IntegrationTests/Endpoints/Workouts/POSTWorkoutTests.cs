using Api.IntegrationTests.Builders;
using Api.Models.Workouts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace Api.IntegrationTests.Endpoints.Workouts;

public class POSTWorkoutTests : BaseTestFixture
{
    [Test]
    public async Task WhenExercisesPropertyIsMissing_ShouldReturnBadRequest()
    {
        var request = new { notes = "Test", totalDurationMinutes = 30, workoutDate = DateTime.UtcNow.AddHours(-1) };
        var response = await PostWorkoutAnon(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises", "The Exercises field is required.");
    }

    [Test]
    public async Task WhenExercisesIsNull_ShouldReturnBadRequest()
    {
        var request = new WorkoutRequest
        {
            Exercises = null,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises", "The Exercises field is required.");
    }

    [Test]
    public async Task WhenExercisesIsEmpty_ShouldReturnBadRequest()
    {
        var request = new WorkoutRequest
        {
            Exercises = [],
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises", "The field Exercises must be a string or array type with a minimum length of '1'.");
    }

    [Test]
    public async Task WhenTotalDurationMinutesPropertyIsMissing_ShouldReturnBadRequest()
    {
        var exercises = new[] { new { name = Guid.NewGuid(), sets = new[] { new { repetitions = 10, weightKg = 0.0 } } } };
        var request = new { exercises, notes = "Test", workoutDate = DateTime.UtcNow.AddHours(-1) };
        var response = await PostWorkoutAnon(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "TotalDurationMinutes", "The TotalDurationMinutes field is required.");
    }

    [Test]
    public async Task WhenTotalDurationMinutesIsNull_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 0.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = null,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "TotalDurationMinutes", "The TotalDurationMinutes field is required.");
    }

    [Test]
    public async Task WhenTotalDurationMinutesIsZero_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 0.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 0,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "TotalDurationMinutes", "The field TotalDurationMinutes must be between 1 and 2147483647.");
    }

    [Test]
    public async Task WhenTotalDurationMinutesIsNegative_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 0.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = -5,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "TotalDurationMinutes", "The field TotalDurationMinutes must be between 1 and 2147483647.");
    }

    [Test]
    public async Task WhenWorkoutDatePropertyIsMissing_ShouldReturnBadRequest()
    {
        var exercises = new[] { new { name = Guid.NewGuid(), sets = new[] { new { repetitions = 10, weightKg = 0.0 } } } };
        var request = new { exercises, notes = "Test", totalDurationMinutes = 30 };
        var response = await PostWorkoutAnon(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "WorkoutDate", "The WorkoutDate field is required.");
    }

    [Test]
    public async Task WhenWorkoutDateIsNull_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 0.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = null
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "WorkoutDate", "The WorkoutDate field is required.");
    }

    [Test]
    public async Task WhenWorkoutDateIsInTheFuture_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 0.0 }] } };
        var futureDate = DateTime.UtcNow.AddHours(1);
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = futureDate
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "WorkoutDate", "Workout date cannot be in the future.");
    }

    [Test]
    public async Task WhenExerciseIdIsNull_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = null, Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 0.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Id", "The Id field is required.");
    }

    [Test]
    public async Task WhenMultipleExercisesAndOneHasInvalidSets_ShouldReturnBadRequest()
    {
        var exercises = new[]
        {
            new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 0.0 }] },
            new Exercise { Id = Guid.NewGuid(), Sets = [] } // Invalid: empty sets array
        };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[1].Sets", "The field Sets must be a string or array type with a minimum length of '1'.");
    }

    [Test]
    public async Task WhenMultipleExercisesAndOneHasNullId_ShouldReturnBadRequest()
    {
        var exercises = new[]
        {
            new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 0.0 }] },
            new Exercise { Id = null, Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 0.0 }] }
        };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[1].Id", "The Id field is required.");
    }

    [Test]
    public async Task WhenExerciseSetsIsNull_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = null } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Sets", "The Sets field is required.");
    }

    [Test]
    public async Task WhenExerciseSetsIsEmpty_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Sets", "The field Sets must be a string or array type with a minimum length of '1'.");
    }

    [Test]
    public async Task WhenSetRepetitionsIsNull_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = null, WeightKg = 0.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Sets[0].Repetitions", "The Repetitions field is required.");
    }

    [Test]
    public async Task WhenMultipleSetsAndOneHasInvalidRepetitions_ShouldReturnBadRequest()
    {
        var exercises = new[]
        {
            new Exercise { Id = Guid.NewGuid(), Sets = new[]
            {
                new WorkoutSet { Repetitions = 10, WeightKg = 0.0 },
                new WorkoutSet { Repetitions = 0, WeightKg = 0.0 } // Invalid: zero repetitions
            }}
        };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Sets[1].Repetitions", "The field Repetitions must be between 1 and 2147483647.");
    }

    [Test]
    public async Task WhenMultipleSetsAndOneHasNullWeight_ShouldReturnBadRequest()
    {
        var exercises = new[]
        {
            new Exercise { Id = Guid.NewGuid(), Sets = new[]
            {
                new WorkoutSet { Repetitions = 10, WeightKg = 0.0 },
                new WorkoutSet { Repetitions = 10, WeightKg = null } // Invalid: null weight
            }}
        };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Sets[1].WeightKg", "The WeightKg field is required.");
    }

    [Test]
    public async Task WhenSetRepetitionsIsZero_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 0, WeightKg = 0.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Sets[0].Repetitions", "The field Repetitions must be between 1 and 2147483647.");
    }

    [Test]
    public async Task WhenSetRepetitionsIsNegative_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = -5, WeightKg = 0.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Sets[0].Repetitions", "The field Repetitions must be between 1 and 2147483647.");
    }

    [Test]
    public async Task WhenSetWeightKgIsNull_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = null }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Sets[0].WeightKg", "The WeightKg field is required.");
    }

    [Test]
    public async Task WhenSetWeightKgIsNegative_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = -5.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[0].Sets[0].WeightKg", "The field WeightKg must be between 0 and 1.7976931348623157E+308.");
    }

    [Test]
    public async Task WhenMultipleExercisesWithMultipleSetsAndOneSetIsInvalid_ShouldReturnBadRequest()
    {
        var exercises = new[]
        {
            new Exercise { Id = Guid.NewGuid(), Sets = new[]
            {
                new WorkoutSet { Repetitions = 10, WeightKg = 0.0 },
                new WorkoutSet { Repetitions = 15, WeightKg = 5.0 }
            }},
            new Exercise { Id = Guid.NewGuid(), Sets = new[]
            {
                new WorkoutSet { Repetitions = 20, WeightKg = 20.0 },
                new WorkoutSet { Repetitions = 0, WeightKg = 20.0 } // Invalid: zero repetitions
            }},
            new Exercise { Id = Guid.NewGuid(), Sets = new[]
            {
                new WorkoutSet { Repetitions = 5, WeightKg = 50.0 },
                new WorkoutSet { Repetitions = 5, WeightKg = 50.0 }
            }}
        };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 60,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises[1].Sets[1].Repetitions", "The field Repetitions must be between 1 and 2147483647.");
    }

    [Test]
    public async Task WhenNoExercisesInDatabase_ShouldReturnBadRequest()
    {
        var exercises = new[] { new Exercise { Id = Guid.NewGuid(), Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 10 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        AssertErrorExists(await GetErrorResponse(response), "Exercises", "Exercises with IDs " + exercises[0].Id + " not found.");
    }

    [Test]
    public async Task WhenExerciseDoesNotExistInDatabase_ShouldReturnBadRequest()
    {
        Guid pushupId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<WorkoutContext>();
            db.Exercises.Add(ExerciseTableBuilder
                .Create()
                .WithId(pushupId)
                .WithName("Pushups")
                .Build());
            db.Exercises.Add(ExerciseTableBuilder
                .Create()
                .WithName("Squats")
                .Build());
            await db.SaveChangesAsync();
        }

        var exercises = new[]
        {
            new Exercise { Id = pushupId, Sets = new[]
            {
                new WorkoutSet { Repetitions = 10, WeightKg = 1.0 },
                new WorkoutSet { Repetitions = 15, WeightKg = 5.0 }
            }},
            new Exercise { Id = Guid.NewGuid(), Sets = new[]
            {
                new WorkoutSet { Repetitions = 20, WeightKg = 20.0 },
                new WorkoutSet { Repetitions = 5, WeightKg = 20.0 }
            }},
            new Exercise { Id = Guid.NewGuid(), Sets = new[]
            {
                new WorkoutSet { Repetitions = 5, WeightKg = 50.0 },
                new WorkoutSet { Repetitions = 5, WeightKg = 50.0 }
            }}
        };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };
        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var responseDetails = await GetErrorResponse(response);
        AssertErrorExists(responseDetails, "Exercises", "Exercises with IDs " + exercises[1].Id + ", " + exercises[2].Id + " not found.");
    }

    [Test]
    public async Task WhenSingleValidActivity_ShouldReturnCreated()
    {
        Guid pushupId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<WorkoutContext>();
            db.Exercises.Add(ExerciseTableBuilder
                .Create()
                .WithId(pushupId)
                .WithName("Pushups")
                .Build());
            await db.SaveChangesAsync();
        }

        var exercises = new[] { new Exercise { Id = pushupId, Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 5.0 }] } };
        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 30,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };

        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        //Assert that the workout was actually created in the database
        using (var scope = factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<WorkoutContext>();
            var workoutInDb = await db.Workouts
                .Include(w => w.WorkoutActivities)
                .ThenInclude(wa => wa.Exercise)
                .FirstOrDefaultAsync();

            Assert.That(workoutInDb, Is.Not.Null, "Workout should exist in the database");
            Assert.That(workoutInDb!.WorkoutActivities.Count, Is.EqualTo(1), "Workout should have one activity");
            var activity = workoutInDb.WorkoutActivities.First();
            Assert.That(activity.Exercise.Id, Is.EqualTo(pushupId), "Exercise ID should match");
            Assert.That(activity.Repetitions, Is.EqualTo(10), "Set repetitions should match");
            Assert.That(activity.WeightKg, Is.EqualTo(5.0), "Set weight should match");
        }
    }

    [Test]
    public async Task MultipleValidActivities_ShouldReturnCreated()
    {
        Guid pushupId = Guid.NewGuid();
        Guid squatId = Guid.NewGuid();
        Guid plankId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<WorkoutContext>();
            db.Exercises.Add(ExerciseTableBuilder
                .Create()
                .WithId(pushupId)
                .WithName("Pushups")
                .Build());
            db.Exercises.Add(ExerciseTableBuilder
                .Create()
                .WithId(squatId)
                .WithName("Squats")
                .Build());
            db.Exercises.Add(ExerciseTableBuilder
                .Create()
                .WithId(plankId)
                .WithName("Plank")
                .Build());
            await db.SaveChangesAsync();
        }

        var repeatedExercises = new[]
        {
            new Exercise { Id = plankId, Sets = [new WorkoutSet { Repetitions = 30, WeightKg = 0.0 }] },
            new Exercise { Id = plankId, Sets = [new WorkoutSet { Repetitions = 20, WeightKg = 0.0 }, new WorkoutSet { Repetitions = 10, WeightKg = 5.0 }] }
        };

        var noneRepeatedExercises = new[]
        {
            new Exercise { Id = pushupId, Sets = [new WorkoutSet { Repetitions = 10, WeightKg = 5.0 }] },
            new Exercise { Id = squatId, Sets = [new WorkoutSet { Repetitions = 15, WeightKg = 10.0 }] }
        };
        var exercises = noneRepeatedExercises.Concat(repeatedExercises).ToArray();

        var request = new WorkoutRequest
        {
            Exercises = exercises,
            Notes = "Test",
            TotalDurationMinutes = 45,
            WorkoutDate = DateTime.UtcNow.AddHours(-1)
        };

        var response = await PostWorkout(request);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        //Assert that the workout was actually created in the database
        using (var scope = factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<WorkoutContext>();
            var workoutInDb = await db.Workouts
                .Include(w => w.WorkoutActivities)
                .ThenInclude(wa => wa.Exercise)
                .FirstOrDefaultAsync();

            Assert.That(workoutInDb, Is.Not.Null, "Workout should exist in the database");
            Assert.That(workoutInDb!.WorkoutActivities.Count, Is.EqualTo(5), "Workout should have five activities");

            foreach (var exercise in noneRepeatedExercises)
            {
                var activity = workoutInDb.WorkoutActivities.FirstOrDefault(wa => wa.Exercise.Id == exercise.Id);
                Assert.That(activity, Is.Not.Null, $"Activity for exercise ID {exercise.Id} should exist");
                Assert.That(activity!.Repetitions, Is.EqualTo(exercise.Sets!.First().Repetitions), "Set repetitions should match");
                Assert.That(activity.WeightKg, Is.EqualTo(exercise.Sets!.First().WeightKg), "Set weight should match");
            }

            foreach (var exercise in repeatedExercises)
            {
                foreach (var set in exercise.Sets!)
                {
                    var activity = workoutInDb.WorkoutActivities.FirstOrDefault(wa =>
                        wa.Exercise.Id == exercise.Id &&
                        wa.Repetitions == set.Repetitions &&
                        wa.WeightKg == set.WeightKg);
                    Assert.That(activity, Is.Not.Null, $"Activity for exercise ID {exercise.Id} should exist");
                    Assert.That(activity!.Repetitions, Is.EqualTo(set.Repetitions), "Set repetitions should match");
                    Assert.That(activity.WeightKg, Is.EqualTo(set.WeightKg), "Set weight should match");
                }
            }
        }
    }

    private async Task<HttpResponseMessage> PostWorkout(WorkoutRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await client.PostAsync("/workouts", content);
    }

    private async Task<HttpResponseMessage> PostWorkoutAnon(object request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await client.PostAsync("/workouts", content);
    }

    private async Task<ValidationProblemDetails?> GetErrorResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ValidationProblemDetails>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private void AssertErrorExists(ValidationProblemDetails? errorResponse, string fieldName, string expectedErrorMessage)
    {
        Assert.That(errorResponse, Is.Not.Null, "Error response should not be null");
        Assert.That(errorResponse?.Errors, Contains.Key(fieldName), $"Error field '{fieldName}' should exist");
        Assert.That(errorResponse?.Errors[fieldName], Does.Contain(expectedErrorMessage),
            $"Error message for '{fieldName}' should contain '{expectedErrorMessage}'");
    }
}
