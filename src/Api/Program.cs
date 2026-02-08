using System.Text.Json.Serialization;
using Api.Converters;
using Api.Extensions;
using Core.ExtensionMethods;
using Infrastructure.Database.ExtensionMethods;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication().AddJwtBearer();
builder.SetupLogging();
builder.Services.AddOpenApi();
builder.Services.AddValidation();
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    options.SerializerOptions.Converters.Add(new JsonDateTimeOffsetUtcConverter());
});

builder.Services.AddDbContext(builder.Configuration.GetConnectionString("WorkoutContext")!);
builder.Services.AddHealthChecks().AddDbContextHealthCheck();
builder.Services.AddRepositories();

builder.Services.AddServices();
builder.Services.AddQueries();
builder.Services.AddControllers();

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
}

app.SetupDatabase();

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();