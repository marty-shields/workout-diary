using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

[TestFixture]
public class BaseTestFixture
{
    internal TestApplicationFactory factory;
    internal HttpClient client;

    [SetUp]
    public void SetUp()
    {
        factory = new TestApplicationFactory();
        client = factory.CreateClient();
        using (var scope = factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<WorkoutContext>();
            db.Database.EnsureCreated();
        }
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        factory.Dispose();
    }
}
