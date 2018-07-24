using System;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AutoIncrementAttribute : AttributeBase
    {
    }
}