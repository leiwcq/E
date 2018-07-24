using System;

namespace E.Interface.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PreDropTableAttribute : AttributeBase
    {
        public string Sql { get; set; }

        public PreDropTableAttribute(string sql)
        {
            Sql = sql;
        }
    }
}