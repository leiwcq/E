using System;

namespace E.Serializer
{
    public interface IStringSerializer
    {
        TTo DeserializeFromString<TTo>(string serializedText);
        object DeserializeFromString(string serializedText, Type type);
        string SerializeToString<TFrom>(TFrom from);
    }
}
