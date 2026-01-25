using System.Text.Json;
using System.Text.Json.Serialization;
using Core.ValueObjects.Exercise;

namespace Infrastructure.Database.SeedData;

public class EquipmentEnumTypeConverter : JsonConverter<Equipment>
{
    private Dictionary<Equipment, string> _equipmentToString = new()
    {
        { Equipment.BodyOnly, "body only" },
        { Equipment.Machine, "machine" },
        { Equipment.Other, "other" },
        { Equipment.FoamRoll, "foam roll" },
        { Equipment.Kettlebells, "kettlebells" },
        { Equipment.Dumbbell, "dumbbell" },
        { Equipment.Cable, "cable" },
        { Equipment.Barbell, "barbell" },
        { Equipment.Bands, "bands" },
        { Equipment.MedicineBall, "medicine ball" },
        { Equipment.ExerciseBall, "exercise ball" },
        { Equipment.EZCurlBar, "e-z curl bar" }
    };

    public override Equipment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();
        if (enumString == null)
        {
            throw new JsonException("Equipment value is null");
        }

        return _equipmentToString.First(x => x.Value.Equals(enumString, StringComparison.OrdinalIgnoreCase)).Key;
    }

    public override void Write(Utf8JsonWriter writer, Equipment value, JsonSerializerOptions options)
        => writer.WriteStringValue(_equipmentToString[value]);
}
