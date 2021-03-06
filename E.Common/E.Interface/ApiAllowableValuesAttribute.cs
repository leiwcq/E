﻿using System;

namespace E.Interface
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ApiAllowableValuesAttribute : AttributeBase
    {
        public ApiAllowableValuesAttribute(string name)
        {
            Name = name;
        }
        public ApiAllowableValuesAttribute(string name, int min, int max) : this(name)
        {
            Type = "RANGE";
            Min = min;
            Max = max;
        }

        public ApiAllowableValuesAttribute(string name, params string[] values)
            : this(name)
        {
            Type = "LIST";
            Values = values;
        }

        public ApiAllowableValuesAttribute(string name, Type enumType)
            : this(name)
        {
#if NETFX_CORE || NETSTANDARD1_1 || PORTABLE7
			if (enumType.GetTypeInfo().IsEnum)
#else
            if (enumType.IsEnum)
#endif
            {
                Type = "LIST";
                Values = Enum.GetNames(enumType);
            }
        }

        public ApiAllowableValuesAttribute(string name, Func<string[]> listAction)
            : this(name)
        {
            if (listAction != null)
            {
                Type = "LIST";
                Values = listAction();
            }
        }
        /// <summary>
        /// Gets or sets parameter name with which allowable values will be associated.
        /// </summary>
        public string Name { get; set; }

        public string Type { get; set; }

        public int? Min { get; set; }

        public int? Max { get; set; }

        public String[] Values { get; set; }

        //TODO: should be implemented according to:
        //https://github.com/wordnik/swagger-core/wiki/datatypes
    }
}
