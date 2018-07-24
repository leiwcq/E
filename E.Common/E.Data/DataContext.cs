using E.Configurations;

namespace E.Data
{
    internal class DataConfig
    {
        public string DefaultConnectionString { get; set; }
    }

    internal static class DataContext
    {
        private static AssemblyConfiguration<DataConfig> _configuration;

        internal static AssemblyConfiguration<DataConfig> Configuration =>
            _configuration ?? (_configuration = new AssemblyConfiguration<DataConfig>("EData.json",
                new DataConfig
                {
                    DefaultConnectionString = "server=localhost;database=data;user=root;password=123456"
                }));


        /// <summary>
        /// 默认连接字符串
        /// </summary>
        public static string DefaultConnectionString => Configuration.Root.DefaultConnectionString;

        public static string ConnectionString(string name)
        {
            return Configuration.Get(name);
        }
    }
}
