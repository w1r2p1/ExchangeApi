using ExchangeApi.Core;
using ExchangeApi.Huobi.Core;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Swap
{
    public class KlineWebSocketClient : SwapPublicWebSocketClientBase
    {
        public string ContractCode { get; }

        public string Period { get; }

        protected override string SubscribeKey => $"market.{ContractCode}.kline.{Period}";

        public KlineWebSocketClient(string code, string period, string host, IWebProxy proxy = null) : base(host, proxy)
        {
            this.ContractCode = code.ToUpper();
            this.Period = period;
        }

        public async Task<string> Request(DateTime from, DateTime to) {
            

            try
            {
                WebSocketRequestBuilder builder = new WebSocketRequestBuilder()
                .AddParam("from", (int)(from.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds)
                .AddParam("to", (int)(to.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds);

                var response = await SendRequest(SubscribeKey, builder, CancellationToken.None);

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
