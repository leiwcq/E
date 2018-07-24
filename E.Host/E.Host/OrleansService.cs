using PeterKottas.DotNetCore.WindowsService.Interfaces;

namespace E.Host
{
    public class OrleansService:IMicroService
    {

        public void Start()
        {
            Services.OrleansService.Start().Wait();
        }

        public void Stop()
        {
            Services.OrleansService.Stop().Wait();
        }
    }
}
