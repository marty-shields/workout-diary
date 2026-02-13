using System.Net.Http.Headers;
using System.Security.Claims;
using Api.IntegrationTests.Builders;
using Core.AggregateRoots;
using Infrastructure.Database;
using Infrastructure.Database.SeedData;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

[Parallelizable]
[TestFixture]
public class BaseTestFixture
{
    internal TestApplicationFactory factory;
    internal HttpClient client;
    internal IEnumerable<Exercise> seededExercises = Array.Empty<Exercise>();

    [SetUp]
    public async Task SetUp()
    {
        factory = new TestApplicationFactory();
        client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<WorkoutContext>();
            await context.Database.EnsureCreatedAsync();
            seededExercises = await ExerciseDataSeeder.SeedListFromJsonAsync("exercises.json", context, CancellationToken.None);
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        client.Dispose();
        using (var scope = factory.Services.CreateScope())
        {
            await scope.ServiceProvider
                .GetRequiredService<WorkoutContext>()
                .Database
                .EnsureDeletedAsync();
        }
        await factory.DisposeAsync();
    }

    internal void AddJWTTokenToRequest(string subject)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTokenBuilder.Create()
                .WithClaim(new Claim("sub", subject))
                .Build());
    }

}
