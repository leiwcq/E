using System;
using System.Threading.Tasks;
using E.Logging;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using Orleans.Hosting.Development;
using Orleans.Runtime.Configuration;

namespace E.Host.Services
{
    public static class OrleansService
    {
        private static ISiloHost _host;

        public static async Task Start()
        {
            // define the cluster configuration
            var config = new ClusterConfiguration
            {
                Globals =
                {
                    AdoInvariant = ServiceContext.Invariant,
                    DataConnectionString = ServiceContext.OrleansConnectionString,
                    LivenessType = GlobalConfiguration.LivenessProviderType.AdoNet,
                    ClusterId = ServiceContext.ClusterId,
                    ServiceId = Guid.Empty,
                    ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AdoNet,
                    AdoInvariantForReminders = ServiceContext.Invariant,
                    DataConnectionStringForReminders = ServiceContext.OrleansConnectionString,
                },
                PrimaryNode = null
            };
            config.Globals.SeedNodes.Clear();

            //config.Globals.RegisterBootstrapProvider<>();
            //config.AddMemoryStorageProvider();
            var siloPort = ServiceContext.SiloPort;
            var gatewayPort = ServiceContext.GatewayPort;

            Console.WriteLine($"准备监听 SiloPort:{siloPort} GatewayPort:{gatewayPort}");

            var builder = new SiloHostBuilder()
                .ConfigureEndpoints(ServiceContext.HostName, siloPort, gatewayPort, listenOnAnyHostAddress: true)
                .UseConfiguration(config)
                .AddAdoNetGrainStorageAsDefault(options =>
                {
                    options.Invariant = ServiceContext.Invariant;
                    options.ConnectionString = ServiceContext.OrleansConnectionString;
                    options.UseJsonFormat = true;
                })
                .UseDashboard(options => { options.Port = ServiceContext.DashboardPort; })
                .ConfigureApplicationParts(parts => parts.AddFromAppDomain().AddFromApplicationBaseDirectory())
                .ConfigureLogging(logging =>
                {
                    logging.AddNLog();
                    //logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .AddMemoryGrainStorage("PubSubStore")
                //.AddSimpleMessageStreamProvider("AccountBalanceStreamProvider")
                .UseInClusterTransactionManager()
                .UseInMemoryTransactionLog()
                .UseTransactionalState();
                //.AddStartupTask(
                //    async (services, cancellation) =>
                //    {
                //        var isLog = ExtAccountContext.IsLog;
                //    });

            _host = builder.Build();
            await _host.StartAsync();
        }

        public static async Task Stop()
        {
            await _host.StopAsync();
        }
    }
}
