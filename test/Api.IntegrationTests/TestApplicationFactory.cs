using Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = $"User ID=postgres;Password=password;Host=localhost;Port=5432;Database={Guid.CreateVersion7()};";
        builder.ConfigureTestServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                options.TokenValidationParameters.RequireSignedTokens = false;
            });

            services.ConfigureDbContext<WorkoutContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
        });
    }
}