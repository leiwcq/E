using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using E.User.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace E.User.Repository
{
    public class UserRepository:IDisposable
    {
        public UserRepository()
        {
            using (var userContext = new UserContext())
            {
                userContext.Database.EnsureCreated();
            }
        }

        public async Task<Models.User> Register(Models.User user)
        {
            using (var userContext = new UserContext())
            {
                var findUser = await userContext.Users.AsQueryable().FirstOrDefaultAsync(f => f.UserName.Equals(user.UserName));
                if (findUser == null)
                {
                    var dbUser = await userContext.Users.AddAsync(user);
                    await userContext.SaveChangesAsync();
                    return dbUser.Entity;
                }

                return default(Models.User);
            }
        }

        public async Task<Models.Account> GetAccount(long userId)
        {
            using (var userContext = new UserContext())
            {
                var findAccount = await userContext.Accounts.AsQueryable()
                    .FirstOrDefaultAsync(f => f.UserId.Equals(userId));
                if (findAccount == null)
                {
                    var dbAccount = await userContext.Accounts.AddAsync(new Models.Account
                    {
                        UserId = userId,
                        Balance = 0,
                        AccountType = 0
                    });
                    await userContext.SaveChangesAsync();
                    
                    await userContext.AccountChangeLogs.AddAsync(new Models.AccountChangeLog
                    {
                        UserId = userId,
                        AccountId = dbAccount.Entity.AccountId,
                        AfterBalance = 0,
                        BeforeBalance = 0,
                        ChangeType = AccountChangeType.Initialization,
                        SchemeId = IdentityGeneration.IdentityGeneration.BuildSingleId("AII"),
                        Description = "初始化用户账户"
                    });
                    await userContext.SaveChangesAsync();
                }

                return findAccount;
            }
        }

        public async Task<Models.Account> Recharge(long userId, string schemeId, decimal money)
        {
            var strUserId = userId.ToString("D");
            Monitor.Enter(strUserId);
            using (var userContext = new UserContext())
            {
                var findAccount = await userContext.Accounts.FindAsync(userId);
                if (findAccount != null)
                {
                    findAccount.Balance += money;
                    await userContext.AccountChangeLogs.AddAsync(new Models.AccountChangeLog
                    {
                        UserId = userId,
                        AccountId = findAccount.AccountId,
                        AfterBalance = findAccount.Balance,
                        BeforeBalance = findAccount.Balance - money,
                        ChangeType = AccountChangeType.Recharge,
                        SchemeId = IdentityGeneration.IdentityGeneration.BuildSingleId("RCI"),
                        Description = $"向用户{userId} 账户{findAccount.AccountId} 充值{money:C}"
                    });
                    await userContext.SaveChangesAsync();
                }
                Monitor.Exit(strUserId);
                return findAccount;
            }

            
        }

        public async Task<Models.Account> Payment(long userId, string schemeId, decimal money)
        {
            var strUserId = userId.ToString("D");
            Monitor.Enter(strUserId);
            using (var userContext = new UserContext())
            {
                var findAccount = await userContext.Accounts.FindAsync(userId);
                if (findAccount != null)
                {
                    if (findAccount.Balance >= money)
                    {
                        findAccount.Balance -= money;
                        await userContext.AccountChangeLogs.AddAsync(new Models.AccountChangeLog
                        {
                            UserId = userId,
                            AccountId = findAccount.AccountId,
                            AfterBalance = findAccount.Balance,
                            BeforeBalance = findAccount.Balance + money,
                            ChangeType = AccountChangeType.Payment,
                            SchemeId = IdentityGeneration.IdentityGeneration.BuildSingleId("ODI"),
                            Description = $"向用户{userId} 账户{findAccount.AccountId} 扣费{money:C}"
                        });
                        await userContext.SaveChangesAsync();
                    }
                    else
                    {
                        throw new NotSupportedException("用户余额不足");
                    }
                }
                Monitor.Exit(strUserId);
                return findAccount;
            }
        }

        public void Dispose()
        {
        }
    }
}
