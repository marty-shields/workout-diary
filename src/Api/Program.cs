using System.Text.Json.Serialization;
using Api.Extensions;
using Core.ExtensionMethods;
using Infrastructure.Database;
using Infrastructure.Database.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.SetupLogging();
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddDbContext<WorkoutContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("WorkoutContext"));
});

builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddQueries();

WebApplication app = builder.Build();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGroup("/workouts").MapWorkoutsApi();

app.Run();