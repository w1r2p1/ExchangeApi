using ExchangeApi.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Core
{
    public abstract class SwapPublicWebSocketClientBase : SwapWebSocketClientBase
    {
        private readonly Dictionary<string, string> _messagePool = new Dictionary<string, string>(1);

        public SwapPublicWebSocketClientBase(string host, IWebProxy proxy = null) : base(host, "/swap-ws", proxy)
        {

        }

        protected override async Task OnTextMessage(string str)
        {
            JObject jo = JObject.Parse(str);
            if (jo.TryGetValue("ping", out var ping))
            {
                await OnPing(ping.ToObject<long>());
            }
            else if (jo.TryGetValue("ch", out var ch))
            {
                OnPush(ch.ToObject<string>(), str);
            }
            else if (jo.TryGetValue("rep", out var rep))
            {
                var clientid = jo["id"].ToObject<string>();
                string key = GetRequestKey(rep.ToObject<string>(), clientid);
                _messagePool.Add(key, str);
            }
            else if (jo.TryGetValue("subbed", out var subbed))
            {
                var clientid = jo["id"].ToObject<string>();
                string key = GetSubscribeKey(subbed.ToObject<string>(), clientid);
                _messagePool.Add(key, str);
            }
            else {
                throw new NotImplementedException(str);
            }
        }

        private async Task OnPing(long ts) {
            WebSocketRequestBuilder builder = new WebSocketRequestBuilder()
                .AddParam("pong", ts);
            await this.SendAsync(builder.Build(), CancellationToken.None);
        }

        protected override async Task<string> SendRequest(string key, WebSocketRequestBuilder builder, CancellationToken cancellationToken = default) {
            string clientid = Guid.NewGuid().ToString();

            string reqkey = GetRequestKey(key, clientid);

            builder = (builder ?? new WebSocketRequestBuilder()).AddParam("req", key).AddParam("id", clientid);
            string json = builder.Build();
            await SendAsync(json, cancellationToken);

            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (_messagePool.TryGetValue(reqkey, out string response))
                    {
                        _messagePool.Remove(reqkey);
                        return response;
                    }
                }
            }, cancellationToken);

            var repJson = await task;
            return repJson;
        }

        protected override async Task<string> SendSubscribe(string key, WebSocketRequestBuilder builder, CancellationToken cancellationToken = default)
        {
            string clientid = Guid.NewGuid().ToString();
            string subkey = GetSubscribeKey(key, clientid);
            builder = (builder ?? new WebSocketRequestBuilder()).AddParam("sub", key).AddParam("id", clientid);

            string json = builder.Build();
            await SendAsync(json, cancellationToken);

            var task = Task.Factory.StartNew(() => {
                while (true) {
                    Thread.Sleep(10);
                    if (_messagePool.TryGetValue(subkey, out string response)) {
                        _messagePool.Remove(subkey);
                        return response;
                    }
                }
            }, cancellationToken);

            var repJson = await task;

            return repJson;
        }

        protected override async Task<string> SendUnsubscribe(string key, CancellationToken cancellationToken = default)
        {
            JObject jo = new JObject();
            jo["status"] = "ok";
            return jo.ToString();
        }

        protected override void CheckResultAndThrow(string response)
        {
            JToken jt = JToken.Parse(response);
            string status = jt["status"].ToObject<string>();
            if (!status.Equals("ok"))
            {
                throw new Exception(response);
            }
        }

        private string GetRequestKey(string key, string clientid)
        {
            return $"req::{key}::{clientid}";
        }

        private string GetSubscribeKey(string key, string clientid) {
            return $"sub::{key}::{clientid}";
        }
    }
}
