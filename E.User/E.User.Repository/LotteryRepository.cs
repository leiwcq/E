using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using E.User.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace E.User.Repository
{
    public class LotteryRepository
    {
        public async Task<Models.Scheme> Betting(Models.Scheme scheme)
        {
            using (var context = new UserContext())
            {
                var findScheme = await context.Schemes.AsQueryable().FirstOrDefaultAsync(f => f.SchemeId.Equals(scheme.SchemeId));
                if (findScheme == null)
                {
                    var dbScheme = await context.Schemes.AddAsync(scheme);
                    await context.SaveChangesAsync();
                    return dbScheme.Entity;
                }

                return default(Models.Scheme);
            }
        }

        public async Task<Models.Scheme> UpdateSchemePaymentStatus(long schemeIndex, PaymentStatus paymentStatus)
        {
            var strUserId = schemeIndex.ToString("D");
            Monitor.Enter(strUserId);
            using (var context = new UserContext())
            {
                var scheme = await context.Schemes.FindAsync(schemeIndex);
                if (scheme != null)
                {
                    if (scheme.PaymentStatus < paymentStatus)
                    {
                        scheme.PaymentStatus = paymentStatus;
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        throw new NotSupportedException("状态不允许进行修改");
                    }
                }
                Monitor.Exit(strUserId);
                return scheme;
            }
        }
    }
}
