using System;

namespace E.Interface
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NamedConnectionAttribute : AttributeBase
    {
        public string Name { get; set; }

        public NamedConnectionAttribute(string name)
        {
            Name = name;
        }
    }
}