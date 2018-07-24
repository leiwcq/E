using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using E.Common;
using E.Serializer.Common;

namespace E.Serializer
{
    public static class PlatformExtensions
    {
        private static readonly ConcurrentDictionary<Type, ObjectDictionaryDefinition> toObjectMapCache =
            new ConcurrentDictionary<Type, ObjectDictionaryDefinition>();

        internal class ObjectDictionaryDefinition
        {
            public Type Type;
            public readonly List<ObjectDictionaryFieldDefinition> Fields = new List<ObjectDictionaryFieldDefinition>();
            public readonly Dictionary<string, ObjectDictionaryFieldDefinition> FieldsMap = new Dictionary<string, ObjectDictionaryFieldDefinition>();

            public void Add(string name, ObjectDictionaryFieldDefinition fieldDef)
            {
                Fields.Add(fieldDef);
                FieldsMap[name] = fieldDef;
            }
        }

        internal class ObjectDictionaryFieldDefinition
        {
            public string Name;
            public Type Type;

            public GetMemberDelegate GetValueFn;
            public SetMemberDelegate SetValueFn;

            public Type ConvertType;
            public GetMemberDelegate ConvertValueFn;

            public void SetValue(object instance, object value)
            {
                if (SetValueFn == null)
                    return;

                if (!Type.IsInstanceOfType(value))
                {
                    lock (this)
                    {
                        //Only caches object converter used on first use
                        if (ConvertType == null)
                        {
                            ConvertType = value.GetType();
                            ConvertValueFn = TypeConverter.CreateTypeConverter(ConvertType, Type);
                        }
                    }

                    if (ConvertType.IsInstanceOfType(value))
                    {
                        value = ConvertValueFn(value);
                    }
                    else
                    {
                        var tempConvertFn = TypeConverter.CreateTypeConverter(value.GetType(), Type);
                        value = tempConvertFn(value);
                    }
                }

                SetValueFn(instance, value);
            }
        }

        public static Dictionary<string, object> ToObjectDictionary(this object obj)
        {
            if (obj == null)
                return null;

            if (obj is Dictionary<string, object> alreadyDict)
                return alreadyDict;

            if (obj is IDictionary<string, object> interfaceDict)
                return new Dictionary<string, object>(interfaceDict);

            if (obj is Dictionary<string, string> stringDict)
            {
                var to = new Dictionary<string, object>();
                foreach (var entry in stringDict)
                {
                    to[entry.Key] = entry.Value;
                }
                return to;
            }

            var type = obj.GetType();

            if (!toObjectMapCache.TryGetValue(type, out var def))
                toObjectMapCache[type] = def = CreateObjectDictionaryDefinition(type);

            var dict = new Dictionary<string, object>();

            foreach (var fieldDef in def.Fields)
            {
                dict[fieldDef.Name] = fieldDef.GetValueFn(obj);
            }

            return dict;
        }

        public static object FromObjectDictionary(this IReadOnlyDictionary<string, object> values, Type type)
        {
            var alreadyDict = type == typeof(IReadOnlyDictionary<string, object>);
            if (alreadyDict)
                return true;

            if (!toObjectMapCache.TryGetValue(type, out var def))
                toObjectMapCache[type] = def = CreateObjectDictionaryDefinition(type);

            var to = type.CreateInstance();
            foreach (var entry in values)
            {
                if (!def.FieldsMap.TryGetValue(entry.Key, out var fieldDef) &&
                    !def.FieldsMap.TryGetValue(entry.Key.ToPascalCase(), out fieldDef)
                    || entry.Value == null)
                    continue;

                fieldDef.SetValue(to, entry.Value);
            }
            return to;
        }

        public static T FromObjectDictionary<T>(this IReadOnlyDictionary<string, object> values)
        {
            return (T)values.FromObjectDictionary(typeof(T));
        }

        private static ObjectDictionaryDefinition CreateObjectDictionaryDefinition(Type type)
        {
            var def = new ObjectDictionaryDefinition
            {
                Type = type,
            };

            foreach (var pi in type.GetSerializableProperties())
            {
                def.Add(pi.Name, new ObjectDictionaryFieldDefinition
                {
                    Name = pi.Name,
                    Type = pi.PropertyType,
                    GetValueFn = pi.CreateGetter(),
                    SetValueFn = pi.CreateSetter(),
                });
            }

            if (JsConfig.IncludePublicFields)
            {
                foreach (var fi in type.GetSerializableFields())
                {
                    def.Add(fi.Name, new ObjectDictionaryFieldDefinition
                    {
                        Name = fi.Name,
                        Type = fi.FieldType,
                        GetValueFn = fi.CreateGetter(),
                        SetValueFn = fi.CreateSetter(),
                    });
                }
            }
            return def;
        }

        public static Dictionary<string, object> ToSafePartialObjectDictionary<T>(this T instance)
        {
            var to = new Dictionary<string, object>();
            var propValues = instance.ToObjectDictionary();
            if (propValues != null)
            {
                foreach (var entry in propValues)
                {
                    var valueType = entry.Value?.GetType();

                    try
                    {
                        if (valueType == null || !valueType.IsClass || valueType == typeof(string))
                        {
                            to[entry.Key] = entry.Value;
                        }
                        else if (!TypeSerializer.HasCircularReferences(entry.Value))
                        {
                            if (entry.Value is IEnumerable enumerable)
                            {
                                to[entry.Key] = entry.Value;
                            }
                            else
                            {
                                to[entry.Key] = entry.Value.ToSafePartialObjectDictionary();
                            }
                        }
                        else
                        {
                            to[entry.Key] = entry.Value.ToString();
                        }

                    }
                    catch (Exception ignore)
                    {
                        Tracer.Instance.WriteDebug($"Could not retrieve value from '{valueType?.GetType().Name}': ${ignore.Message}");
                    }
                }
            }
            return to;
        }

        public static Dictionary<string, object> MergeIntoObjectDictionary(this object obj, params object[] sources)
        {
            var to = obj.ToObjectDictionary();
            foreach (var source in sources)
                foreach (var entry in source.ToObjectDictionary())
                {
                    to[entry.Key] = entry.Value;
                }
            return to;
        }
    }
}
