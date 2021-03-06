﻿using System;

namespace E.Serializer
{
    /// <summary>
    /// Allow Type to be deserialized into late-bould object Types using __type info
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RuntimeSerializableAttribute : Attribute {}

    /// <summary>
    /// Allow Type to be deserialized into late-bould object Types using __type info
    /// </summary>
    public interface IRuntimeSerializable { }
}