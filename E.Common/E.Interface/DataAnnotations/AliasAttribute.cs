using System;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
    public class AliasAttribute : AttributeBase
    {
        public string Name { get; set; }

        public AliasAttribute(string name)
        {
            Name = name;
        }
    }
}