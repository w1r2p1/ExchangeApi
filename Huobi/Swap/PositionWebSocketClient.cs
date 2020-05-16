using ExchangeApi.Huobi.Core;
using System.Net;

namespace ExchangeApi.Huobi.Swap
{
    /// <summary>
    /// 持仓变动订阅
    /// </summary>
    public class PositionWebSocketClient : SwapPrivateWebSocketClientBase
    {
        public string ContractCode { get; }

        protected override string SubscribeKey => $"positions.{ContractCode}";

        public PositionWebSocketClient(string accesskey, string secretkey, string code, string host, IWebProxy proxy = null) : base(accesskey, secretkey, host, proxy)
        {
            ContractCode = code.ToUpper();
        }
    }
}
