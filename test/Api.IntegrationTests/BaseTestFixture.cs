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
        var token = JwtTokenProvider.JwtSecurityTokenHandler.WriteToken(
            new JwtSecurityToken(
                JwtTokenProvider.Issuer,
                JwtTokenProvider.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: JwtTokenProvider.SigningCredentials
            )
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

}
