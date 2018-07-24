using System;
using System.Threading.Tasks;
using EasyNetQ;
using E.Common;

namespace E.Test.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var gatewayUrl = ExtAccountContext.GatewayUrl;
            var bus = RabbitHutch.CreateBus("host=192.168.1.163;virtualHost=/;username=mqtest;password=123456");

            bus.SubscribeAsync<SimpleMessageA>("Queue_Identifier",
                message => Task.Factory.StartNew(() =>
                {
                    if (message.Data.Contains("8"))
                    {
                        throw new Exception("");
                    }
                    //Console.WriteLine($"Message:{message.Data}");
                }).ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        Console.WriteLine("消息{0}消费成功", message.Data);
                        // Everything worked out ok
                    }
                    else
                    {
                        Console.WriteLine("消息{0}消费失败", message.Data);
                        // Dont catch this, it is caught further up the heirarchy and results in being sent to the default error queue
                        // on the broker
                        throw new EasyNetQException("Message processing exception - look in the default error queue (broker)");
                    }
                }));

            Console.WriteLine("启动成功按任意键退出");
            Console.ReadLine();
        }
    }
}
