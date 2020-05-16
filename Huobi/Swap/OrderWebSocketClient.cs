using ExchangeApi.Huobi.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Swap
{
    public class OrderWebSocketClient : SwapPrivateWebSocketClientBase
    {
        public string ContractCode { get; }

        protected override string SubscribeKey => $"orders.{ContractCode}";

        public OrderWebSocketClient(string accesskey, string secretkey, string code, string host, IWebProxy proxy = null) : base(accesskey, secretkey, host, proxy)
        {
            ContractCode = code.ToUpper();
        }
    }
}
