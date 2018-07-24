using System;
using System.Globalization;
using System.Reflection;
using E.Common;
using E.Extensions;
using E.Serializer.Common;

namespace E.Serializer
{
    public class PclExport
    {
        public static PclExport Instance = new NetStandardPclExport();

        public virtual SetMemberDelegate CreateSetter(FieldInfo fieldInfo)
        {
            return fieldInfo.SetValue;
        }

        public virtual SetMemberDelegate<T> CreateSetter<T>(FieldInfo fieldInfo)
        {
            return (o, x) => fieldInfo.SetValue(o, x);
        }

        public virtual GetMemberDelegate CreateGetter(FieldInfo fieldInfo)
        {
            return fieldInfo.GetValue;
        }

        public virtual GetMemberDelegate<T> CreateGetter<T>(FieldInfo fieldInfo)
        {
            return x => fieldInfo.GetValue(x);
        }

        public virtual SetMemberDelegate CreateSetter(PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.GetSetMethod(nonPublic: true);
            if (propertySetMethod == null) return null;

            return (o, convertedValue) =>
                propertySetMethod.Invoke(o, new[] { convertedValue });
        }

        public virtual SetMemberDelegate<T> CreateSetter<T>(PropertyInfo propertyInfo)
        {
            var propertySetMethod = propertyInfo.GetSetMethod(nonPublic: true);
            if (propertySetMethod == null) return null;

            return (o, convertedValue) =>
                propertySetMethod.Invoke(o, new[] { convertedValue });
        }

        public virtual GetMemberDelegate CreateGetter(PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetGetMethod(nonPublic: true);
            if (getMethodInfo == null) return null;

            return o => propertyInfo.GetGetMethod(nonPublic: true).Invoke(o, TypeConstants.EmptyObjectArray);
        }

        public virtual GetMemberDelegate<T> CreateGetter<T>(PropertyInfo propertyInfo)
        {
            var getMethodInfo = propertyInfo.GetGetMethod(nonPublic: true);
            if (getMethodInfo == null) return null;

            return o => propertyInfo.GetGetMethod(nonPublic: true).Invoke(o, TypeConstants.EmptyObjectArray);
        }

        public virtual string ToXsdDateTimeString(DateTime dateTime)
        {
            return System.Xml.XmlConvert.ToString(dateTime.ToStableUniversalTime(), DateTimeSerializer.XSD_DATE_TIME_FORMAT);
        }

        public virtual string ToLocalXsdDateTimeString(DateTime dateTime)
        {
            return System.Xml.XmlConvert.ToString(dateTime, DateTimeSerializer.XSD_DATE_TIME_FORMAT);
        }

        public virtual DateTime ParseXsdDateTime(string dateTimeStr)
        {
            return System.Xml.XmlConvert.ToDateTimeOffset(dateTimeStr).DateTime;
        }

        public virtual DateTime ParseXsdDateTimeAsUtc(string dateTimeStr)
        {
            return DateTimeSerializer.ParseManual(dateTimeStr, DateTimeKind.Utc)
                ?? DateTime.ParseExact(dateTimeStr, DateTimeSerializer.XSD_DATE_TIME_FORMAT, CultureInfo.InvariantCulture);
        }

        public virtual DateTime ToStableUniversalTime(DateTime dateTime)
        {
            // Silverlight 3, 4 and 5 all work ok with DateTime.ToUniversalTime, but have no TimeZoneInfo.ConverTimeToUtc implementation.
            return dateTime.ToUniversalTime();
        }

        public virtual ParseStringDelegate GetDictionaryParseMethod<TSerializer>(Type type)
            where TSerializer : ITypeSerializer
        {
            return null;
        }

        public virtual ParseStringSegmentDelegate GetDictionaryParseStringSegmentMethod<TSerializer>(Type type)
            where TSerializer : ITypeSerializer
        {
            return null;
        }

        public virtual ParseStringDelegate GetSpecializedCollectionParseMethod<TSerializer>(Type type)
            where TSerializer : ITypeSerializer
        {
            return null;
        }

        public virtual ParseStringSegmentDelegate GetSpecializedCollectionParseStringSegmentMethod<TSerializer>(Type type)
            where TSerializer : ITypeSerializer
        {
            return null;
        }

        public virtual ParseStringDelegate GetJsReaderParseMethod<TSerializer>(Type type)
            where TSerializer : ITypeSerializer
        {
            return null;
        }

        public virtual ParseStringSegmentDelegate GetJsReaderParseStringSegmentMethod<TSerializer>(Type type)
            where TSerializer : ITypeSerializer
        {
            return null;
        }

        public virtual Type UseType(Type type)
        {
            return type;
        }

        public virtual void RegisterForAot()
        {
        }
    }
}
