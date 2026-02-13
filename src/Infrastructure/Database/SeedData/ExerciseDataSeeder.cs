using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.ValueObjects.Exercise;
using Infrastructure.Database.ExtensionMethods;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.SeedData;

public static class ExerciseDataSeeder
{
    public static void SeedListFromJson(string jsonFilePath, DbContext context)
    {
        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
        var exerciseData = JsonSerializer.Deserialize<List<Exercise>>(
            stream,
            GetJsonSerializerOptions());

        if (ExercideDataExists(exerciseData))
        {
            context.Set<Tables.Exercise>().AddRange(exerciseData!.Select(e => e.ToTable()));
        }

        context.SaveChanges();
    }

    public static async Task<IEnumerable<Core.AggregateRoots.Exercise>> SeedListFromJsonAsync(string jsonFilePath, DbContext context, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read);
        var exerciseData = await JsonSerializer.DeserializeAsync<List<Exercise>>(
            stream,
            GetJsonSerializerOptions(),
            cancellationToken);

        if (ExercideDataExists(exerciseData))
        {
            var exerciseTable = exerciseData!.Select(e => e.ToTable());
            await context.Set<Tables.Exercise>().AddRangeAsync(exerciseTable, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return exerciseTable.Select(et => et.ToEntity());
        }

        return Array.Empty<Core.AggregateRoots.Exercise>();
    }

    public static async Task SeedListFromJsonAsync(IEnumerable<Exercise>? exercises, DbContext context, CancellationToken cancellationToken)
    {

    }

    private static bool ExercideDataExists(IEnumerable<Exercise>? exerciseData) => exerciseData is not null && exerciseData.Any();

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

    public class Exercise
    {
        public required string Name { get; set; }
        public Force? Force { get; set; }
        public required Level Level { get; set; }
        public Mechanic? Mechanic { get; set; }
        public Equipment? Equipment { get; set; }
        public required ICollection<Muscle> PrimaryMuscles { get; set; }
        public ICollection<Muscle>? SecondaryMuscles { get; set; }
        public required ICollection<string> Instructions { get; set; }
        public required Category Category { get; set; }

        public Tables.Exercise ToTable()
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(Name));
            Guid id = new Guid(hash);

            return new Tables.Exercise
            {
                Id = id,
                Name = Name,
                Force = Force,
                Level = Level,
                Mechanic = Mechanic,
                Equipment = Equipment,
                PrimaryMuscles = PrimaryMuscles,
                SecondaryMuscles = SecondaryMuscles,
                Instructions = Instructions,
                Category = Category
            };
        }
    }

}
