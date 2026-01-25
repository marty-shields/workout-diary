using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Database.Tables;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.SeedData;

public static class ExerciseDataSeeder
{
    public static void SeedListFromJson(string jsonFilePath, DbContext context)
    {
        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
        var exerciseData = JsonSerializer.Deserialize<List<Exercise>>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (ExercideDataExists(exerciseData))
        {
            context.Set<Exercise>().AddRange(exerciseData!);
        }

        context.SaveChanges();
    }

    public static async Task SeedListFromJsonAsync(string jsonFilePath, DbContext context, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
        var exerciseData = await JsonSerializer.DeserializeAsync<List<Exercise>>(
            stream,
            GetJsonSerializerOptions(),
            cancellationToken);

        if (ExercideDataExists(exerciseData))
        {
            await context.Set<Exercise>().AddRangeAsync(exerciseData!, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool ExercideDataExists(List<Exercise>? exerciseData) => exerciseData is not null && exerciseData.Any();

    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        jsonSerializerOptions.Converters.Add(new EquipmentEnumTypeConverter());
        jsonSerializerOptions.Converters.Add(new MuscleEnumTypeConverter());
        jsonSerializerOptions.Converters.Add(new CategoryEnumTypeConverter());
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        return jsonSerializerOptions;
    }
}
