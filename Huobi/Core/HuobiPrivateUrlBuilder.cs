using ExchangeApi.Core;
using System;

namespace ExchangeApi.Huobi.Core
{
    public class HuobiPrivateUrlBuilder : PrivateUrlBuilder
    {
        public HuobiPrivateUrlBuilder(string accessKey, string secretKey, string host) : base(accessKey, secretKey, host)
        {
            _signer = new HuobiSigner(secretKey);
        }

        public override string Build(string method, string path, DateTime utcDateTime, GetRequestBuilder request)
        {
            string strDateTime = utcDateTime.ToString("s");

            var req = new GetRequestBuilder(request)
                .AddParam(Constants.AKey, _accessKey)
                .AddParam(Constants.SMKey, Constants.SMValue)
                .AddParam(Constants.SVKey, Constants.SVValue)
                .AddParam(Constants.TKey, strDateTime);

            string param = req.Build();

            string signature = _signer.Sign(method, _host, path, param);

            string url = $"http://{_host}{path}?{param}&{Constants.SKey}={Uri.EscapeDataString(signature)}";

            return url;
        }
    }
}
