using ExchangeApi.Core;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeApi.Huobi.Core
{
    public abstract class SwapWebSocketClientBase : WebSocketClientBase
    {

        private event Action<string> handler = null;

        private bool isSubbed = false;

        protected abstract string SubscribeKey { get; }

        public SwapWebSocketClientBase(string host, string path, IWebProxy proxy = null) : base(Encoding.UTF8, host, path, proxy) { }

        protected override async Task OnBinaryMessage(byte[] data)
        {
            string json = GZipDecompresser.Decompress(data);
            await OnTextMessage(json);
        }

        protected void OnPush(string key, string json) {
            handler?.Invoke(json);
        }

        protected abstract Task<string> SendRequest(string key, WebSocketRequestBuilder builder, CancellationToken cancellationToken);

        protected abstract Task<string> SendSubscribe(string key, WebSocketRequestBuilder builder, CancellationToken cancellationToken);

        protected abstract Task<string> SendUnsubscribe(string key, CancellationToken cancellationToken);

        public async Task Subcribe(Action<string> handler, CancellationToken cancellationToken) {
            this.handler += handler;
            if (isSubbed) return;

            WebSocketRequestBuilder requestBuilder = GetRequestBuilder();
            var response = await SendSubscribe(SubscribeKey, requestBuilder, cancellationToken);
            try
            {
                CheckResultAndThrow(response);
                isSubbed = true;
                return;
            }
            catch
            {
                this.handler -= handler;
                isSubbed = false;
                throw;
            }
        }

        public async Task Unsubscribe(Action<string> handler) {
            this.handler -= handler;
            if (this.handler != null) return;
            var response = await SendUnsubscribe(SubscribeKey, CancellationToken.None);
            try
            {
                CheckResultAndThrow(response);
                isSubbed = false;
            }
            catch
            {
                this.handler += handler;
                isSubbed = true;
                throw;
            }
        }

        protected virtual WebSocketRequestBuilder GetRequestBuilder() {
            return null;
        }

        protected abstract void CheckResultAndThrow(string response);
    }
}
