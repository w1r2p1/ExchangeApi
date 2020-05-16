using System;
using System.Collections.Generic;
using System.Text;

namespace ExchangeApi.Core
{
    public abstract class PrivateUrlBuilder
    {
        protected readonly string _host;
        protected readonly string _accessKey;
        protected readonly string _secretKey;
        protected ISigner _signer;

        public PrivateUrlBuilder(string accessKey, string secretKey, string host) {
            _accessKey = accessKey;
            _secretKey = secretKey;
            _host = host;
        }

        public string Build(string method, string path) {
            return Build(method, path, DateTime.UtcNow, null);
        }

        public string Build(string method, string path, DateTime utcDateTime) {
            return Build(method, path, utcDateTime, null);
        }

        public string Build(string method, string path, GetRequestBuilder request) {
            return Build(method, path, DateTime.UtcNow, request);
        }

        public abstract string Build(string method, string path, DateTime utcDateTime, GetRequestBuilder request);
    }
}
