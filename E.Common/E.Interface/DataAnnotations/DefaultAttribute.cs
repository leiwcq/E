using System;
using System.Globalization;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DefaultAttribute : AttributeBase
    {
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }

        public Type DefaultType { get; set; }
        public string DefaultValue { get; set; }

        public bool OnUpdate { get; set; }

        public DefaultAttribute(int intValue)
        {
            IntValue = intValue;
            DefaultType = typeof(int);
            DefaultValue = IntValue.ToString();
        }

        public DefaultAttribute(double doubleValue)
        {
            DoubleValue = doubleValue;
            DefaultType = typeof(double);
            DefaultValue = doubleValue.ToString(CultureInfo.CurrentCulture);
        }

        public DefaultAttribute(string defaultValue)
        {
            DefaultType = typeof(string);
            DefaultValue = defaultValue;
        }

        public DefaultAttribute(Type defaultType, string defaultValue)
        {
            DefaultValue = defaultValue;
            DefaultType = defaultType;
        }
    }
}