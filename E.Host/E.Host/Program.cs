using System;
using PeterKottas.DotNetCore.WindowsService;

namespace E.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceRunner<OrleansService>.Run(config =>
            {
                var name = config.GetDefaultName();
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((extraArguments, controller) => new OrleansService());
                    serviceConfig.OnStart((service, extraArguments) =>
                    {
                        Console.WriteLine("Service {0} started", name);
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                        Console.WriteLine("Service {0} stopped", name);
                        service.Stop();
                    });

                    serviceConfig.OnInstall(service =>
                    {
                        Console.WriteLine("Service {0} installed", name);
                    });

                    serviceConfig.OnUnInstall(service =>
                    {
                        Console.WriteLine("Service {0} uninstalled", name);
                    });

                    serviceConfig.OnPause(service =>
                    {
                        Console.WriteLine("Service {0} paused", name);
                    });

                    serviceConfig.OnContinue(service =>
                    {
                        Console.WriteLine("Service {0} continued", name);
                    });

                    serviceConfig.OnError(e =>
                    {
                        Console.WriteLine("Service {0} errored with exception : {1}", name, e.Message);
                    });
                });
                config.SetName("E Orleans Host Service");
                config.SetDescription("E Orleans Host Service");
                config.SetDisplayName("E Service Host");
            });
            //Console.WriteLine("按任意键退出程序");
            //Console.ReadLine();
        }
    }
}
