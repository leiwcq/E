using Microsoft.EntityFrameworkCore;

namespace E.Data.MySql
{
    public abstract class MySqlContextBase:ContextBase
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(ConnectionString);
        }
    }
}
