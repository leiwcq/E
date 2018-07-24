using System.ComponentModel;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace E.Data
{
    public abstract class ContextBase : DbContext
    {
        public static long InstanceCount;

        protected string ConnectionString { get; }

        protected ContextBase():this(string.Empty)
        {
            Interlocked.Increment(ref InstanceCount);
        }

        protected ContextBase(string name)
        {
            Interlocked.Increment(ref InstanceCount);

            if (string.IsNullOrWhiteSpace(name))
            {
                name = GetType().Name;
            }

            ConnectionString = DataContext.ConnectionString(name);
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                ConnectionString = DataContext.DefaultConnectionString;
            }
        }

        public virtual void Initialization()
        {
            Database.EnsureCreated();
        }
    }
}
