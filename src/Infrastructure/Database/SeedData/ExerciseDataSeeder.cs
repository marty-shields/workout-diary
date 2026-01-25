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
        var deserializedData = JsonSerializer.Deserialize<List<Exercise>>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (deserializedData != null)
        {
            foreach (var data in deserializedData)
            {
                context.Set<Exercise>().Add(data);
            }
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

        foreach (var exercise in exerciseData!)
        {
            context.Set<Exercise>().Add(exercise);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        jsonSerializerOptions.Converters.Add(new EquipmentEnumTypeConverter());
        jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        return jsonSerializerOptions;
    }
}
