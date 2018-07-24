using System;
using System.Collections.Generic;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class CompositeKeyAttribute : AttributeBase
    {
        public CompositeKeyAttribute()
        {
            FieldNames = new List<string>();
        }

        public CompositeKeyAttribute(params string[] fieldNames)
        {
            FieldNames = new List<string>(fieldNames);
        }

        public List<string> FieldNames { get; set; }
    }
}