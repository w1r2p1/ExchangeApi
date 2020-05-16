using ExchangeApi.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Core
{
    public abstract class SwapPrivateWebSocketClientBase : SwapWebSocketClientBase
    {
        protected const string PATH = "/swap-notification";

        private const string _aKKey = Constants.AKey;
        private const string _sMKey = Constants.SMKey;
        private const string _sMValue = Constants.SMValue;
        private const string _sVKey = Constants.SVKey;
        private const string _sVValue = Constants.SVValue;
        private const string _tKey = Constants.TKey;
        private const string _sKey = Constants.SKey;

        private string _accesskey;

        private ISigner _signer;

        GetRequestBuilder _builder;

        private readonly Dictionary<string, string> _messagePool = new Dictionary<string, string>(1);

        public SwapPrivateWebSocketClientBase(string accesskey, string secretkey, string host, IWebProxy proxy = null) : base(host, "/swap-notification", proxy)
        {
            _accesskey = accesskey;
            _signer = new HuobiSigner(secretkey);

            _builder = new GetRequestBuilder()
                .AddParam(_aKKey, _accesskey)
                .AddParam(_sMKey, _sMValue)
                .AddParam(_sVKey, _sVValue);
        }

        protected override async Task OnTextMessage(string str)
        {
            var jt = JToken.Parse(str);
            string op = jt["op"].ToObject<string>();

            if (string.Equals(op, "ping"))
            {
                long ts = jt["ts"].ToObject<long>();
                await OnPing(ts);
            }
            else if (string.Equals(op, "auth"))
            {
                _messagePool.Add(GetAuthKey(), str);
            }
            else if (string.Equals(op, "notify"))
            {
                string topic = jt["topic"].ToObject<string>();
                OnPush(topic, str);
            }
            else if (string.Equals(op, "sub"))
            {
                string clientid = jt["cid"].ToObject<string>();
                string topic = jt["topic"].ToObject<string>();
                string subkey = GetSubscribeKey(topic, clientid);
                _messagePool.Add(subkey, str);
            }
            else if (string.Equals(op, "unsub"))
            {
                string clientid = jt["cid"].ToObject<string>();
                string topic = jt["topic"].ToObject<string>();
                string unsubkey = GetUnsubscribeKey(topic, clientid);
                _messagePool.Add(unsubkey, str);
            }
        }

        public async Task Auth(CancellationToken cancellationToken = default)
        {
            string authkey = GetAuthKey();

            DateTime now = DateTime.UtcNow;

            string signature = GetSignature(now);

            WebSocketRequestBuilder builder2 = new WebSocketRequestBuilder()
                .AddParam("op", "auth")
                .AddParam("type", "api")
                .AddParam(_aKKey, _accesskey)
                .AddParam(_sMKey, _sMValue)
                .AddParam(_sVKey, _sVValue)
                .AddParam(_tKey, now.ToString("s"))
                .AddParam(_sKey, signature);

            await this.SendAsync(builder2.Build(), cancellationToken);

            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (_messagePool.TryGetValue(authkey, out string response))
                    {
                        _messagePool.Remove(authkey);
                        return response;
                    }
                }
            }, cancellationToken);

            var repJson = await task;

            JToken jt = JToken.Parse(repJson);
            int err_code = jt["err-code"].ToObject<int>();
            if (err_code != 0) throw new Exception($"{GetType()}.{nameof(Auth)} failed: {jt["err-msg"].ToObject<string>()}({err_code})");
        }

        private async Task OnPing(long ts)
        {
            WebSocketRequestBuilder builder = new WebSocketRequestBuilder()
                .AddParam("op", "pong")
                .AddParam("ts", ts);
            await this.SendAsync(builder.Build(), CancellationToken.None);
        }

        protected override Task<string> SendRequest(string key, WebSocketRequestBuilder builder, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override async Task<string> SendSubscribe(string key, WebSocketRequestBuilder builder, CancellationToken cancellationToken = default)
        {
            string clientid = Guid.NewGuid().ToString();
            string subkey = GetSubscribeKey(key, clientid);
            builder = (builder ?? new WebSocketRequestBuilder())
                .AddParam("op", "sub")
                .AddParam("cid", clientid)
                .AddParam("topic", key);

            string json = builder.Build();
            await SendAsync(json, cancellationToken);

            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (_messagePool.TryGetValue(subkey, out string response))
                    {
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
            string clientid = Guid.NewGuid().ToString();
            string unsubkey = GetUnsubscribeKey(key, clientid);
            WebSocketRequestBuilder builder = new WebSocketRequestBuilder()
                .AddParam("op", "unsub")
                .AddParam("cid", clientid)
                .AddParam("topic", key);

            string json = builder.Build();
            await SendAsync(json, cancellationToken);

            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(10);
                    if (_messagePool.TryGetValue(unsubkey, out string response))
                    {
                        _messagePool.Remove(unsubkey);
                        return response;
                    }
                }
            }, cancellationToken);

            var repJson = await task;
            return repJson;
        }

        protected override void CheckResultAndThrow(string response)
        {
            JToken jt = JToken.Parse(response);
            int errCode = jt["err-code"].ToObject<int>();
            if (errCode != 0)
            {
                throw new Exception(response);
            }
        }

        private string GetSubscribeKey(string key, string clientid)
        {
            return $"sub::{key}::{clientid}";
        }

        private string GetUnsubscribeKey(string key, string clientid)
        {
            return $"unsub::{key}::{clientid}";
        }

        private string GetAuthKey()
        {
            return $"auth";
        }

        private string GetSignature(DateTime utcNow)
        {
            GetRequestBuilder builder = new GetRequestBuilder(_builder);
            builder.AddParam(_tKey, utcNow.ToString("s"));
            string signature = _signer.Sign(Constants.GET_METHOD, Host, Path, builder.Build());
            return signature;
        }
    }
}
