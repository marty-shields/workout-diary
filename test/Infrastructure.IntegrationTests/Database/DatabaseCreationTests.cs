using System.Text.Json;
using Infrastructure.Database;
using Infrastructure.Database.SeedData;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Database;

[TestFixture]
public class DatabaseCreationTests
{
    [Test]
    public async Task DatabaseIsCreated_Successfully()
    {
        var connectionString = $"User ID=postgres;Password=password;Host=localhost;Port=5432;Database={Guid.CreateVersion7()};";
        var options = new DbContextOptionsBuilder<WorkoutContext>()
            .UseNpgsql(connectionString)
            .Options;

        using var context = new WorkoutContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        var canConnect = await context.Database.CanConnectAsync();
        Assert.That(canConnect, Is.True);

        await context.Database.EnsureDeletedAsync();
    }

    [Test]
    public async Task ValidExerciseSeedJsonFile_DataIsSeeded()
    {
        var connectionString = $"User ID=postgres;Password=password;Host=localhost;Port=5432;Database={Guid.CreateVersion7()};";
        var filePath = "Database/SeedData/exercises.json";
        var options = new DbContextOptionsBuilder<WorkoutContext>()
            .UseNpgsql(connectionString)
            .UseAsyncSeeding(async (context, _, cancellationToken)
                => await ExerciseDataSeeder.SeedListFromJsonAsync(filePath, context, cancellationToken))
            .Options;

        using var context = new WorkoutContext(options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var expectedExercises = await JsonSerializer.DeserializeAsync<List<ExerciseDataSeeder.Exercise>>(
            stream,
            ExerciseDataSeeder.GetJsonSerializerOptions());
        Assert.That(expectedExercises, Is.Not.Null);
        Assert.That(expectedExercises.Count, Is.EqualTo(1));

        var expectedExercise = expectedExercises[0].ToTable();
        var actualExercise = await context.Exercises.Where(e => e.Name == expectedExercise.Name).FirstOrDefaultAsync();

        Assert.That(actualExercise, Is.Not.Null);
        Assert.That(actualExercise.Id, Is.EqualTo(expectedExercise.Id));
        Assert.That(actualExercise.Name, Is.EqualTo(expectedExercise.Name));
        Assert.That(actualExercise.Force, Is.EqualTo(expectedExercise.Force));
        Assert.That(actualExercise.Level, Is.EqualTo(expectedExercise.Level));
        Assert.That(actualExercise.Mechanic, Is.EqualTo(expectedExercise.Mechanic));
        Assert.That(actualExercise.Equipment, Is.EqualTo(expectedExercise.Equipment));
        Assert.That(actualExercise.PrimaryMuscles, Is.EqualTo(expectedExercise.PrimaryMuscles));
        Assert.That(actualExercise.SecondaryMuscles, Is.EqualTo(expectedExercise.SecondaryMuscles));
        Assert.That(actualExercise.Instructions, Is.EqualTo(expectedExercise.Instructions));
        Assert.That(actualExercise.Category, Is.EqualTo(expectedExercise.Category));

        await context.Database.EnsureDeletedAsync();
    }
}
