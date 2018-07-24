using System;

namespace E.Interface.DataAnnotations 
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AutoIdAttribute : AttributeBase
    {
    }
}