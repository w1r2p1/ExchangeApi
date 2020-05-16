using ExchangeApi.Core;
using ExchangeApi.Huobi.Core;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Swap
{
    public class PositionClient : HttpClientBase
    {
        private readonly HuobiPrivateUrlBuilder _urlBuilder;

        public PositionClient(string accesskey, string sercetkey, string host, IWebProxy proxy = null) : base(proxy)
        {
            _urlBuilder = new HuobiPrivateUrlBuilder(accesskey, sercetkey, host);
        }

        public async Task<string> GetPositionAsync(string contract_code)
        {
            string url = _urlBuilder.Build(Constants.POST_METHOD, "/swap-api/v1/swap_position_info");
            JObject jo = new JObject();
            jo.Add("contract_code", contract_code);
            var response = await PostAsync(url, jo.ToString());
            return response;
        }

        public async Task<string> GetPositionsAsync()
        {
            string url = _urlBuilder.Build(Constants.POST_METHOD, "/swap-api/v1/swap_position_info");
            var response = await PostAsync(url);
            return response;
        }
    }
}
