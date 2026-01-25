using System.Text.Json;
using System.Text.Json.Serialization;
using Core.ValueObjects.Exercise;

namespace Infrastructure.Database.SeedData;

public class CategoryEnumTypeConverter : JsonConverter<Category>
{
    private Dictionary<Category, string> _categoryToString = new()
    {
        { Category.Strength, "strength" },
        { Category.Stretching, "stretching" },
        { Category.Plyometrics, "plyometrics" },
        { Category.Strongman, "strongman" },
        { Category.Powerlifting, "powerlifting" },
        { Category.Cardio, "cardio" },
        { Category.OlympicWeightlifting, "olympic weightlifting" }
    };

    public override Category Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();
        if (enumString == null)
        {
            throw new JsonException("Category value is null");
        }

        return _categoryToString.First(x => x.Value.Equals(enumString, StringComparison.OrdinalIgnoreCase)).Key;
    }

    public override void Write(Utf8JsonWriter writer, Category value, JsonSerializerOptions options)
        => writer.WriteStringValue(_categoryToString[value]);
}
