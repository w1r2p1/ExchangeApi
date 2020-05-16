using ExchangeApi.Huobi.Core;
using System.Net;

namespace ExchangeApi.Huobi.Swap
{
    /// <summary>
    /// 资产变动订阅
    /// </summary>
    public class AccountWebSocketClient : SwapPrivateWebSocketClientBase
    {
        public string ContractCode { get; }

        protected override string SubscribeKey => $"accounts.{ContractCode}";

        public AccountWebSocketClient(string accesskey, string secretkey, string code, string host, IWebProxy proxy = null) : base(accesskey, secretkey, host, proxy)
        {
            ContractCode = code.ToUpper();
        }
    }
}
