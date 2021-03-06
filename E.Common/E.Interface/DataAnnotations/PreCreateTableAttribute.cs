﻿using System;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PreCreateTableAttribute : AttributeBase
    {
        public string Sql { get; set; }

        public PreCreateTableAttribute(string sql)
        {
            Sql = sql;
        }
    }
}