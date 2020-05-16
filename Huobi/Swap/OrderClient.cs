using ExchangeApi.Core;
using ExchangeApi.Huobi.Core;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Swap
{
    public class OrderClient : HttpClientBase
    {
        private readonly HuobiPrivateUrlBuilder _urlBuilder;



        public OrderClient(string accesskey, string sercetkey, string host, IWebProxy proxy = null) : base(proxy)
        {
            _urlBuilder = new HuobiPrivateUrlBuilder(accesskey, sercetkey, host);
        }

        public async Task<string> PlaceOrderAsync(string contract_code, double price, long volume, string direction, string offset, int lever_rate, string order_price_type) {
            string url = _urlBuilder.Build(Constants.POST_METHOD, "/swap-api/v1/swap_order");
            JObject jo = new JObject();
            jo.Add("contract_code", contract_code);
            if (!(order_price_type.StartsWith("opponent") || order_price_type.StartsWith("optimal"))) {
                jo.Add("price", price);
            }
            jo.Add("volume", volume);
            jo.Add("direction", direction);
            jo.Add("offset", offset);
            jo.Add("lever_rate", lever_rate);
            jo.Add("order_price_type", order_price_type);
            var response = await PostAsync(url, jo.ToString());
            return response;
        }

        public async Task<string> CancelOrderAsync(string order_id, string contract_code) {
            string url = _urlBuilder.Build(Constants.POST_METHOD, "/swap-api/v1/swap_cancel");
            JObject jo = new JObject();
            jo.Add("contract_code", contract_code);
            jo.Add("order_id", order_id);
            var response = await PostAsync(url, jo.ToString());
            return response;
        }

        public async Task<string> GetOrderInfoAsync(string order_id, string contract_code) {
            string url = _urlBuilder.Build(Constants.POST_METHOD, "/swap-api/v1/swap_order_info");
            JObject jo = new JObject();
            jo.Add("contract_code", contract_code);
            jo.Add("order_id", order_id);
            var response = await PostAsync(url, jo.ToString());
            return response;
        }
    }
}
