﻿using System;

namespace E.Interface.DataAnnotations
{
    // https://msdn.microsoft.com/en-us/library/ms184391.aspx
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlServerCollateAttribute : AttributeBase
    {
        public SqlServerCollateAttribute(string collation) { Collation = collation; }

        public string Collation { get; set; }
    }
}