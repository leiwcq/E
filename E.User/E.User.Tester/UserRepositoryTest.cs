using System;
using System.Threading.Tasks;
using E.User.Repository;
using NUnit.Framework;

namespace E.User.Tester
{
    [TestFixture]
    public class UserRepositoryTest
    {
        [Test]
        public Task TestRegisterUser()
        {
            Parallel.For(0, 100, async i =>
            {
                Console.WriteLine($"准备新建用户:TU{i:D6}");
                using (var userRepository = new UserRepository())
                {
                    var user = await userRepository.Register(new Repository.Models.User
                    {
                        UserName = $"TU{i:D6}",
                        SecretKey = Guid.NewGuid().ToString("N"),
                        PassWord = "123456"
                    });
                    await userRepository.GetAccount(user.UserId);
                    await userRepository.Recharge(user.UserId, "", 1000);
                    await userRepository.Payment(user.UserId, "", 10);
                }
            });

            return Task.CompletedTask;
        }

        [Test]
        public async Task TestPayment()
        {
            using (var userRepository = new UserRepository())
            {
                await userRepository.Recharge(1, "", 10000);
            }

            for (var i = 0; i < 1000; i++)
            {
                using (var userRepository = new UserRepository())
                {
                    await userRepository.Payment(1, "", 10);
                }
            }
        }
    }
}
