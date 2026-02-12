using Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Api.IntegrationTests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = $"User ID=postgres;Password=password;Host=localhost;Port=5432;Database={Guid.CreateVersion7()};";
        builder.ConfigureTestServices(services =>
        {
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Configuration = new OpenIdConnectConfiguration
                {
                    Issuer = JwtTokenProvider.Issuer,
                };
                options.TokenValidationParameters.ValidIssuer = JwtTokenProvider.Issuer;
                options.TokenValidationParameters.ValidAudience = JwtTokenProvider.Audience;
                options.Configuration.SigningKeys.Add(JwtTokenProvider.SecurityKey);
            });

            services.ConfigureDbContext<WorkoutContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });
        });
    }
}