using System;
using System.Collections.Generic;
using System.Text;

namespace ExchangeApi.Core
{
    public abstract class PublicUrlBuilder
    {
        private readonly string _host;

        public PublicUrlBuilder(string host) {
            _host = host;
        }

        public virtual string Build(string path, GetRequestBuilder request = null) {
            string url = $"https://{_host}{path}";
            if (request != null) {
                url = $"{url}?{request.Build()}";
            }
            return url;
        }
    }
}
