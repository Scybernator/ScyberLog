using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScyberLog
{
    //https://github.com/dotnet/runtime/issues/43026#issuecomment-949966701
    public class JsonExceptionConverter<TExceptionType> : JsonConverter<TExceptionType>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Exception).IsAssignableFrom(typeToConvert);
        }

        public override TExceptionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException("Deserializing exceptions is not allowed");
        }

        public override void Write(Utf8JsonWriter writer, TExceptionType value, JsonSerializerOptions options)
        {
            if(value == null) { return; }

            var serializableProperties = value.GetType()
                .GetProperties()
                .Where(x => x.Name != nameof(Exception.TargetSite))
                .Select(x => ( x.Name, Value: x.GetValue(value)));

            if (options?.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
            {
                serializableProperties = serializableProperties.Where(x => x.Value != null);
            }

            if(value is AggregateException)
            {
                //Remove InnerException on AggregateException, since it will apear in InnerExceptions
                serializableProperties = serializableProperties.Where(x => x.Name != nameof(Exception.InnerException));
            }

            var propList = serializableProperties.ToList();

            if (propList.Count == 0)
            {
                return; // Nothing to write
            }

            //Add exception type
            propList.Insert(0, ("Type", value.GetType().FullName));

            writer.WriteStartObject();

            foreach (var prop in propList)
            {
                writer.WritePropertyName(options?.PropertyNamingPolicy?.ConvertName(prop.Name) ?? prop.Name);
                JsonSerializer.Serialize(writer, prop.Value, options);
            }

            writer.WriteEndObject();
        }
    }
}