using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using E.Http;
using NUnit.Framework;

namespace E.Tester
{
    [TestFixture]
    public class HttpTest
    {
        [Test]
        public async Task Test()
        {
            var postData =
                "{\"$type\":\"E.UCenter.ExtAccount.Entities.GPGetRequest, E.UCenter.ExtAccount\",\"limit\":0,\"gameId\":\"ENG-MZD-001\",\"uuid\":\"pad_A5kjd5e5dd7rwwf511cc30464f1bccp7\",\"imei\":\"SNCAIPIAOTEST7\",\"cpOrderId\":\"PAD20180626184045985201\",\"signMsg\":\"d790e6517471eb5a442cc78260f947d1\"}";
            var result = await postData.PostGetAsync("http://192.168.1.163:903/get/gameplayer/ponints",
                contentType: "application/json");
            Console.WriteLine(result);
        }
    }
}
