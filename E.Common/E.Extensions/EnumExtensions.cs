using System;
using System.Collections.Generic;
using E.Common;

namespace E.Extensions
{
    public static class EnumExtensions
    {
        public static string ToDescription(this Enum @enum)
        {
            var type = @enum.GetType();

            var memInfo = type.GetMember(@enum.ToString());

            if (memInfo.Length > 0)
            {
                var description = memInfo[0].GetDescription();

                if (description != null)
                    return description;
            }

            return @enum.ToString();
        }

        public static List<string> ToList(this Enum @enum)
        {
            return new List<string>(Enum.GetNames(@enum.GetType()));
        }

        public static TypeCode GetTypeCode(this Enum @enum)
        {
            return Enum.GetUnderlyingType(@enum.GetType()).GetTypeCode();
        }

        public static bool Has<T>(this Enum @enum, T value)
        {
            var typeCode = @enum.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Byte:
                    return (((byte)(object)@enum & (byte)(object)value) == (byte)(object)value);
                case TypeCode.Int16:
                    return (((short)(object)@enum & (short)(object)value) == (short)(object)value);
                case TypeCode.Int32:
                    return (((int)(object)@enum & (int)(object)value) == (int)(object)value);
                case TypeCode.Int64:
                    return (((long)(object)@enum & (long)(object)value) == (long)(object)value);
                default:
                    throw new NotSupportedException($"Enums of type {@enum.GetType().Name}");
            }
        }

        public static bool Is<T>(this Enum @enum, T value)
        {
            var typeCode = @enum.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Byte:
                    return (byte)(object)@enum == (byte)(object)value;
                case TypeCode.Int16:
                    return (short)(object)@enum == (short)(object)value;
                case TypeCode.Int32:
                    return (int)(object)@enum == (int)(object)value;
                case TypeCode.Int64:
                    return (long)(object)@enum == (long)(object)value;
                default:
                    throw new NotSupportedException($"Enums of type {@enum.GetType().Name}");
            }
        }

        public static T Add<T>(this Enum @enum, T value)
        {
            var typeCode = @enum.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Byte:
                    return (T)(object)((byte)(object)@enum | (byte)(object)value);
                case TypeCode.Int16:
                    return (T)(object)((short)(object)@enum | (short)(object)value);
                case TypeCode.Int32:
                    return (T)(object)((int)(object)@enum | (int)(object)value);
                case TypeCode.Int64:
                    return (T)(object)((long)(object)@enum | (long)(object)value);
                default:
                    throw new NotSupportedException($"Enums of type {@enum.GetType().Name}");
            }
        }

        public static T Remove<T>(this Enum @enum, T value)
        {
            var typeCode = @enum.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Byte:
                    return (T)(object)((byte)(object)@enum & ~(byte)(object)value);
                case TypeCode.Int16:
                    return (T)(object)((short)(object)@enum & ~(short)(object)value);
                case TypeCode.Int32:
                    return (T)(object)((int)(object)@enum & ~(int)(object)value);
                case TypeCode.Int64:
                    return (T)(object)((long)(object)@enum & ~(long)(object)value);
                default:
                    throw new NotSupportedException($"Enums of type {@enum.GetType().Name}");
            }
        }

    }

}