using System.Text.Json;
using System.Text.Json.Serialization;

namespace XamlTest.Transport;

public class DpiScaleSerializer : JsonSerializer<DpiScale>
{
    private static JsonSerializerOptions? StaticOptions { get; }


    public override JsonSerializerOptions? Options => StaticOptions;

    static DpiScaleSerializer()
    {
        StaticOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        StaticOptions.Converters.Add(new DpiScaleJsonConverter());
    }

    private class DpiScaleJsonConverter : JsonConverter<DpiScale>
    {
        public override DpiScale Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            reader.Read(); //Start object

            double dpiX = 0.0;
            double dpiY = 0.0;

            ReadDoubleProperty(ref reader, out string? property1, out double value1);
            ReadDoubleProperty(ref reader, out string? property2, out double value2);

            switch (property1)
            {
                case nameof(DpiScale.DpiScaleX):
                    dpiX = value1;
                    break;
                case nameof(DpiScale.DpiScaleY):
                    dpiY = value1;
                    break;
            }
            switch (property2)
            {
                case nameof(DpiScale.DpiScaleX):
                    dpiX = value2;
                    break;
                case nameof(DpiScale.DpiScaleY):
                    dpiY = value2;
                    break;
            }

            reader.Read(); //End object
            return new(dpiX, dpiY);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DpiScale dpiScale,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(DpiScale.DpiScaleX), dpiScale.DpiScaleX);
            writer.WriteNumber(nameof(DpiScale.DpiScaleY), dpiScale.DpiScaleY);
            writer.WriteEndObject();
        }

        private static void ReadDoubleProperty(
            ref Utf8JsonReader reader,
            out string? propertyName,
            out double value)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new InvalidOperationException($"Expected property token but was '{reader.TokenType}'");
            }
            propertyName = reader.GetString();
            reader.Read(); //Read property name
            if (reader.TokenType != JsonTokenType.Number)
            {
                throw new InvalidOperationException($"Expected number token but was '{reader.TokenType}'");
            }
            value = reader.GetDouble();
            reader.Read();
        }
    }
}