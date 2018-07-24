using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace E.Tester {
    [TestFixture]
    public class RabbitMqMessageBusTests {
       
        [Test]
        public async Task TestMessage()
        {
            //var bus = RabbitHutch.CreateBus("host=192.168.1.163;virtualHost=/;username=mqtest;password=123456");



            //for (int i = 0; i < 100; i++)
            //{
            //    await bus.PublishAsync(new SimpleMessageA { Data = $"Hello {i}" });
            //}
            var des = new Security.DES("12345678", 8, Encoding.UTF8);
            var e = des.EncryptToString("adfasfewrwrewrewrewr");
            var k = des.DecryptToString(e);
        }
    }
}
