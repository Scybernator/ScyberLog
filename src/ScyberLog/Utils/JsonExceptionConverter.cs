using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

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
                //Cancellation tokens will throw exceptions on property access if they have been disposed 
                .Where(x => x.PropertyType != typeof(CancellationToken));

            if(value is AggregateException)
            {
                //Remove InnerException on AggregateException, since it will apear in InnerExceptions
                serializableProperties = serializableProperties.Where(x => x.Name != nameof(Exception.InnerException));
            }

            var propertyValues = new List<(string Name, object Value)>();
            foreach(var prop in serializableProperties)
            {
                object val;
                try
                {
                    val = prop.GetValue(value);
                }catch(TargetInvocationException ex)
                {
                    val = ex.InnerException?.Message;
                }
                propertyValues.Add((prop.Name, val));
            }

            if (options?.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
            {
                propertyValues = propertyValues.Where(x => x.Value != null).ToList();
            }

            //Add exception type
            propertyValues.Insert(0, ("Type", value.GetType().FullName));

            writer.WriteStartObject();

            foreach (var (name, val) in propertyValues)
            {
                writer.WritePropertyName(options?.PropertyNamingPolicy?.ConvertName(name) ?? name);
                try
                {
                    JsonSerializer.Serialize(writer, val, options);
                }catch(Exception ex)
                {
                    writer.WriteStringValue("Error serializing property; " + ex.Message);
                }
            }

            writer.WriteEndObject();
        }
    }
}