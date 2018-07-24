using System;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CheckConstraintAttribute : AttributeBase
    {
        public string Constraint { get; }

        public CheckConstraintAttribute(string constraint)
        {
            Constraint = constraint;
        }
    }
}