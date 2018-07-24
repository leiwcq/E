using System;

namespace E.Serializer
{
    public class JsonStringSerializer : IStringSerializer
    {
        public To DeserializeFromString<To>(string serializedText)
        {
            return JsonSerializer.DeserializeFromString<To>(serializedText);
        }

        public object DeserializeFromString(string serializedText, Type type)
        {
            return JsonSerializer.DeserializeFromString(serializedText, type);
        }

        public string SerializeToString<TFrom>(TFrom @from)
        {
            return JsonSerializer.SerializeToString(@from);
        }
    }
}
