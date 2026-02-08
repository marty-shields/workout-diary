using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
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

    internal void AddJWTTokenToRequest(string subject)
    {
        IEnumerable<Claim> claims = [
            new Claim("sub", subject)
        ];
        var token = new JwtSecurityToken(
            issuer: "dotnet-user-jwts",
            audience: "https://localhost:7019",
            claims: claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddHours(1)
        );

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", new JwtSecurityTokenHandler().WriteToken(token));
    }

}
