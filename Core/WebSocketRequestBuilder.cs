using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExchangeApi.Core
{
    public sealed class WebSocketRequestBuilder
    {
        private Dictionary<string, object> _params;

        public WebSocketRequestBuilder(WebSocketRequestBuilder builder = null)
        {
            _params = builder == null ? new Dictionary<string, object>() : new Dictionary<string, object>(builder._params);
        }

        public WebSocketRequestBuilder AddParam(string property, object value)
        {
            if (!string.IsNullOrEmpty(property) && value != null)
            {
                _params.Add(property, value);
            }
            return this;
        }

        public string Build()
        {
            return JsonConvert.SerializeObject(_params);
        }
    }
}
