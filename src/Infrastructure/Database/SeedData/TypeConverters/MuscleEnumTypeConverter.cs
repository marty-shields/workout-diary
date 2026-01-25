using System.Text.Json;
using System.Text.Json.Serialization;
using Core.ValueObjects.Exercise;

namespace Infrastructure.Database.SeedData;

public class MuscleEnumTypeConverter : JsonConverter<Muscle>
{
    private Dictionary<Muscle, string> _muscleToString = new()
    {
        { Muscle.Abdominals, "abdominals" },
        { Muscle.Hamstrings, "hamstrings" },
        { Muscle.Adductors, "adductors" },
        { Muscle.Quadriceps, "quadriceps" },
        { Muscle.Biceps, "biceps" },
        { Muscle.Shoulders, "shoulders" },
        { Muscle.Chest, "chest" },
        { Muscle.MiddleBack, "middle back" },
        { Muscle.Calves, "calves" },
        { Muscle.Glutes, "glutes" },
        { Muscle.LowerBack, "lower back" },
        { Muscle.Lats, "lats" },
        { Muscle.Triceps, "triceps" },
        { Muscle.Traps, "traps" },
        { Muscle.Forearms, "forearms" },
        { Muscle.Neck, "neck" },
        { Muscle.Abductors, "abductors" }
    };

    public override Muscle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();
        if (enumString == null)
        {
            throw new JsonException("Muscle value is null");
        }

        return _muscleToString.First(x => x.Value.Equals(enumString, StringComparison.OrdinalIgnoreCase)).Key;
    }

    public override void Write(Utf8JsonWriter writer, Muscle value, JsonSerializerOptions options)
        => writer.WriteStringValue(_muscleToString[value]);
}
