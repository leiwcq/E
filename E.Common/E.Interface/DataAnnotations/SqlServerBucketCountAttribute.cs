﻿using System;

namespace E.Interface.DataAnnotations
{
    // https://msdn.microsoft.com/en-us/library/dn494956.aspx
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlServerBucketCountAttribute : AttributeBase
    {
        public SqlServerBucketCountAttribute(int count) { Count = count; }

        public int Count { get; set; } 
    }
}