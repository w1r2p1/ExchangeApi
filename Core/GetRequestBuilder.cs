using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExchangeApi.Core
{
    public sealed class GetRequestBuilder
    {
        private Dictionary<string, string> _params;

        public GetRequestBuilder(GetRequestBuilder builder = null)
        {
            _params = builder == null ? new Dictionary<string, string>() : new Dictionary<string, string>(builder._params);
        }

        public GetRequestBuilder AddParam(string key, string value) {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _params.Add(Uri.EscapeDataString(key), Uri.EscapeDataString(value));
            }
            return this;
        }

        public string Build()
        {
            if (_params.Count == 0) return string.Empty;
            var list = _params.OrderBy(i => i.Key, StringComparer.Ordinal).Select(i => $"{i.Key}={i.Value}");
            return string.Join("&", list);
        }
    }
}
