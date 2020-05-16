using ExchangeApi.Core;
using ExchangeApi.Huobi.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Swap
{
    public class KlineClient : HttpClientBase
    {
        private const string PATH = "/swap-ex/market/history/kline";

        private readonly HuobiPublicUrlBuilder _urlBuilder;

        public KlineClient(string host, IWebProxy proxy = null) : base(proxy)
        {
            _urlBuilder = new HuobiPublicUrlBuilder(host);
        }

        public async Task<string> GetKlinesAsync(string symbol, string interval, int count = 150) {
            GetRequestBuilder builder = new GetRequestBuilder().AddParam("contract_code", symbol)
                .AddParam("period", interval)
                .AddParam("size", count.ToString());
            string url = _urlBuilder.Build(PATH, builder);
            var response = await GetAsync(url);
            return response;
        }

        public async Task<string> GetKlinesAsync(string symbol, string interval, DateTime from, DateTime to) {
            GetRequestBuilder builder = new GetRequestBuilder().AddParam("contract_code", symbol)
                .AddParam("period", interval)
                .AddParam("from", ((long)(from.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds).ToString())
                .AddParam("to", ((long)(to.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds).ToString());

            string url = _urlBuilder.Build(PATH, builder);
            var response = await GetAsync(url);
            return response;
        }
    }
}
