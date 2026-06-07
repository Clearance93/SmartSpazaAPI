using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartUtility.Sealed
{
    public class DateOnlyJsonConverter :JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (DateOnly.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }
            throw new JsonException($"Unable to convert \"{value}\" to DateOnly.");
        }
        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}
