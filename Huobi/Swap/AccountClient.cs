using ExchangeApi.Core;
using ExchangeApi.Huobi.Core;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Swap
{
    /// <summary>
    /// 合约资产接口
    /// </summary>
    public class AccountClient : HttpClientBase
    {
        private readonly HuobiPrivateUrlBuilder _urlBuilder;

        public AccountClient(string accesskey, string sercetkey, string host, IWebProxy proxy = null) : base(proxy)
        {
            _urlBuilder = new HuobiPrivateUrlBuilder(accesskey, sercetkey, host);
        }

        public async Task<string> GetAccountAsync(string contract_code)
        {
            string url = _urlBuilder.Build(Constants.POST_METHOD, "/swap-api/v1/swap_account_info");
            JObject jo = new JObject();
            jo.Add("contract_code", contract_code);
            var response = await PostAsync(url, jo.ToString());
            return response;
        }

        public async Task<string> GetAccountsAsync()
        {
            string url = _urlBuilder.Build(Constants.POST_METHOD, "/swap-api/v1/swap_account_info");
            var response = await PostAsync(url);
            return response;
        }
    }
}
