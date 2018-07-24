using E.Configurations;

namespace E.Test.ConsoleApp
{
    public static class ExtAccountContext
    {
        internal class ExtAccountConfig
        {
            public string GatewayUrl { get; set; }
            public string MerchantId { get; set; }
            public string SecretKey { get; set; }
            public string GameId { get; set; }
            public string Rate { get; set; }
            public string Ver { get; set; }
            public string ReceiveUrl { get; set; }
            public string HeartTime { get; set; }
            public string IsLog { get; set; }
        }

        private static AssemblyConfiguration<ExtAccountConfig> _configuration;

        internal static AssemblyConfiguration<ExtAccountConfig> Configuration =>
            _configuration ?? (_configuration = new AssemblyConfiguration<ExtAccountConfig>("ExtGamePointAccount.json",
                new ExtAccountConfig
                {

                    GatewayUrl = "http://116.206.176.112:82",
                    MerchantId = "",
                    SecretKey = "6e8841dd306261c00f98619bc092d45f",
                    GameId = "ENG-BCBM-001",
                    Rate = "100",
                    Ver = "1",
                    ReceiveUrl = "http://116.206.176.112:801",
                    HeartTime = "300",
                    IsLog = "0"
                }));

        private static string _secretKey;

        /// <summary>
        /// 密钥
        /// </summary>
        public static string SecretKey
        {
            get => string.IsNullOrWhiteSpace(_secretKey) ? Configuration.Root.SecretKey : _secretKey;
            set => _secretKey = value;
        }

        /// <summary>
        /// 接口地址
        /// </summary>
        public static string GatewayUrl => Configuration.Root.GatewayUrl;

        /// <summary>
        /// 是否写日志
        /// </summary>
        /// <returns></returns>
        public static bool IsLog
        {
            get
            {
                if (!int.TryParse(Configuration.Root.IsLog, out var i))
                    return false;
                return i > 0;
            }
        }

        /// <summary>
        /// 接收地址
        /// </summary>
        public static string ReceiveUrl => Configuration.Root.ReceiveUrl;

        /// <summary>
        /// 接口商号(暂无);
        /// </summary>
        public static string MerchantId => Configuration.Root.MerchantId;
        /// <summary>
        /// 接口产品ID（上家的与本地采种Id无关）
        /// </summary>
        public static string GameId => Configuration.Root.GameId;

        /// <summary>
        /// 接口网关版本号
        /// </summary>
        public static string Ver => Configuration.Root.Ver;

        /// <summary>
        /// 兑换比率100分
        /// </summary>
        public static decimal Rate
        {
            get
            {
                var str = Configuration.Root.Rate;
                ;
                if (string.IsNullOrEmpty(str)) return 100M;
                if (decimal.TryParse(str, out var rate))
                    return rate;
                return 100M;

            }
        }

        private static string _heartTime = "290";

        /// <summary>
        /// 心跳频率(默认5分钟=300秒) 
        /// </summary>
        public static int HeartTime
        {
            get
            {
                var str = Configuration.Root.HeartTime;
                ;
                if (string.IsNullOrEmpty(str)) return 300;
                if (int.TryParse(str, out var heartTime))
                    return heartTime;
                return 300;
            }
        }
    }
}
