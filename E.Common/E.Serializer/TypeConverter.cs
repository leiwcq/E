using System;
using System.Collections;
using E.Common;

namespace E.Serializer
{
    public static class TypeConverter
    {
        public static GetMemberDelegate CreateTypeConverter(Type fromType, Type toType)
        {
            if (fromType == toType)
                return null;

            if (fromType == typeof(string))
                return fromValue => TypeSerializer.DeserializeFromString((string)fromValue, toType);

            if (toType == typeof(string))
                return TypeSerializer.SerializeToString;

            var underlyingToType = Nullable.GetUnderlyingType(toType) ?? toType;
            var underlyingFromType = Nullable.GetUnderlyingType(fromType) ?? fromType;

            if (underlyingToType.IsEnum)
            {
                if (underlyingFromType.IsEnum || fromType == typeof(string))
                    return fromValue => Enum.Parse(underlyingToType, fromValue.ToString(), true);

                if (underlyingFromType.IsIntegerType())
                    return fromValue => Enum.ToObject(underlyingToType, fromValue);
            }
            else if (underlyingFromType.IsEnum)
            {
                if (underlyingToType.IsIntegerType())
                    return fromValue => Convert.ChangeType(fromValue, underlyingToType, null);
            }
            else if (toType.IsNullableType())
            {
                return null;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(fromType))
            {
                return fromValue =>
                {

                    //TODO
                    return fromValue;
                    //var listResult = TranslateListWithElements.TryTranslateCollections(
                    //    fromType, toType, fromValue);

                    //return listResult ?? fromValue;
                };
            }
            else if (toType.IsValueType)
            {
                return fromValue => Convert.ChangeType(fromValue, toType, null);
            }
            else
            {
                return fromValue =>
                {
                    if (fromValue == null)
                        return fromValue;

                    var toValue = toType.CreateInstance();
                    toValue.PopulateWith(fromValue);
                    return toValue;
                };
            }

            return null;
        }
    }
}
