using System;
using System.Collections.Generic;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class CompositeIndexAttribute : AttributeBase
    {
        public CompositeIndexAttribute()
        {
            FieldNames = new List<string>();
        }

        public CompositeIndexAttribute(params string[] fieldNames)
        {
            FieldNames = new List<string>(fieldNames);
        }

        public CompositeIndexAttribute(bool unique, params string[] fieldNames)
        {
            Unique = unique;
            FieldNames = new List<string>(fieldNames);
        }

        public List<string> FieldNames { get; set; }

        public bool Unique { get; set; }

        public string Name { get; set; }
    }
}