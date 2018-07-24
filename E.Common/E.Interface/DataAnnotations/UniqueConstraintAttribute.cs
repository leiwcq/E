using System;
using System.Collections.Generic;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class UniqueConstraintAttribute : AttributeBase
    {
        public UniqueConstraintAttribute()
        {
            FieldNames = new List<string>();
        }

        public UniqueConstraintAttribute(params string[] fieldNames)
        {
            FieldNames = new List<string>(fieldNames);
        }

        public List<string> FieldNames { get; set; }

        public string Name { get; set; }
    }
}