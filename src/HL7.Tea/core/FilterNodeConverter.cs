using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HL7.Tea.Core
{
    public class FilterNodeConverter: JsonConverter<FilterNode>
    {
        public override FilterNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            // Detect type based on keys
            if (root.TryGetProperty("field", out _))
            {
                return JsonSerializer.Deserialize<Condition>(root.GetRawText(), options);
            }

            if (root.TryGetProperty("and", out _))
            {
                return JsonSerializer.Deserialize<AndGroup>(root.GetRawText(), options);
            }

            if (root.TryGetProperty("or", out _))
            {
                return JsonSerializer.Deserialize<OrGroup>(root.GetRawText(), options);
            }

            if (root.TryGetProperty("not", out _))
            {
                return JsonSerializer.Deserialize<NotGroup>(root.GetRawText(), options);
            }

            throw new JsonException("Invalid filter node");
        }

        public override void Write(Utf8JsonWriter writer, FilterNode value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, options);
        }
    }
}
