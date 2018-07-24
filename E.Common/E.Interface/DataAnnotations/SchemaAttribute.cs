using System;

namespace E.Interface.DataAnnotations
{
    /// <summary>
    /// Used to annotate an Entity with its DB schema
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SchemaAttribute : AttributeBase
    {
        public SchemaAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}