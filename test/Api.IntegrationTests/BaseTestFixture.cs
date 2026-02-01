using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

[Parallelizable]
[TestFixture]
public class BaseTestFixture
{
    internal TestApplicationFactory factory;
    internal HttpClient client;

    [SetUp]
    public async Task SetUp()
    {
        factory = new TestApplicationFactory();
        client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            await scope.ServiceProvider
                .GetRequiredService<WorkoutContext>()
                .Database
                .EnsureCreatedAsync();
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
}
