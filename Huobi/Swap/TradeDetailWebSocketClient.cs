using ExchangeApi.Huobi.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Swap
{
    public class TradeDetailWebSocketClient : SwapPublicWebSocketClientBase
    {
        public string ContractCode { get; }

        protected override string SubscribeKey => $"market.{ContractCode}.trade.detail";

        public TradeDetailWebSocketClient(string code, string host, IWebProxy proxy = null) : base(host, proxy)
        {
            ContractCode = code.ToUpper();
        }

        public async Task<string> Request() {
            try
            {
                var response = await SendRequest(SubscribeKey, null, CancellationToken.None);
                CheckResultAndThrow(response);
                return response;
            }
            catch
            {
                throw;
            }
        }
    }
}
