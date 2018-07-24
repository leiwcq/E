﻿using System;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SqlServerFileTableAttribute : AttributeBase
    {
        public SqlServerFileTableAttribute() { }

        public SqlServerFileTableAttribute(string directory, string collateFileName = null)
        {
            FileTableDirectory = directory;
            FileTableCollateFileName = collateFileName;
        }

        public string FileTableDirectory { get; internal set; }

        public string FileTableCollateFileName { get; internal set; }
    }
}