using Infrastructure.Database;
using Infrastructure.Database.SeedData;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();
builder.Services.AddDbContext<WorkoutContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("WorkoutContext"));
    var exerciseDataFilePath = builder.Configuration.GetValue<string>("DatabaseSeeding:ExerciseDataFilePath");
    if (!string.IsNullOrEmpty(exerciseDataFilePath))
    {
        options.UseAsyncSeeding(async (context, _, cancellationToken) =>
            await ExerciseDataSeeder.SeedListFromJsonAsync(exerciseDataFilePath, context, cancellationToken));
    }

});

WebApplication app = builder.Build();
app.UseHttpsRedirection();
using (var scope = app.Services.CreateScope())
{
    using var context = scope.ServiceProvider.GetRequiredService<WorkoutContext>();
    if (app.Environment.IsDevelopment())
    {
        await context.Database.EnsureDeletedAsync();
    }
    await context.Database.MigrateAsync();
}

app.Run();