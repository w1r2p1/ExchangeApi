using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeApi.Core
{
    public abstract class HttpClientBase
    {
        private IWebProxy proxy;

        public HttpClientBase(IWebProxy proxy = null)
        {
            this.proxy = proxy;
        }

        public async Task<string> GetAsync(string url)
        {
            try
            {
                using (var httpClient = new HttpClient(new HttpClientHandler() { Proxy = proxy }))
                {
                    var response = await httpClient.GetAsync(url);
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        public async Task<string> PostAsync(string url, string body = null, string mediaType = "application/json")
        {
            StringContent httpContent = string.IsNullOrEmpty(body) ? null : new StringContent(body, Encoding.UTF8, mediaType);

            using (var httpClient = new HttpClient(new HttpClientHandler() { Proxy = proxy }))
            {
                var response = await httpClient.PostAsync(url, httpContent);
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }
    }
}
