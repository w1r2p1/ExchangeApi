using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExchangeApi.Core
{
    public abstract class WebSocketClientBase : IDisposable
    {
        public string Host { get; }

        public string Path { get; }

        public Encoding Encoding { get; }

        protected ClientWebSocket _webSocket;

        private Uri uri;

        private CancellationTokenSource _cancellationTokenSource;

        private Task _receiveTask;

        public WebSocketClientBase(Encoding encoding, string host, string path, IWebProxy proxy = null)
        {
            Host = host;
            Path = path;
            Encoding = encoding;

            uri = new Uri($"wss://{host}{path}");

            _webSocket = new ClientWebSocket();
            _webSocket.Options.Proxy = proxy;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await _webSocket.ConnectAsync(uri, cancellationToken);
            _receiveTask = Task.Factory.StartNew(async () =>
            {
                ArraySegment<byte> receiveBuffer = WebSocket.CreateClientBuffer(1024, 1024);

                while (true)
                {
                    WebSocketReceiveResult result = null;
                    List<byte> datas = new List<byte>();
                    do
                    {
                        result = await _webSocket.ReceiveAsync(receiveBuffer, CancellationToken.None);
                        datas.AddRange(receiveBuffer.Array.Take(result.Count));
                    }
                    while (!result.EndOfMessage);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Close:
                            await CloseSelf(result.CloseStatus.Value, result.CloseStatusDescription);
                            break;
                        case WebSocketMessageType.Binary:
                            await OnBinaryMessage(datas.ToArray());
                            break;
                        case WebSocketMessageType.Text:
                            string str = Encoding.GetString(datas.ToArray());
                            await OnTextMessage(str);
                            break;
                    }
                }
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public async Task SendAsync(byte[] data, CancellationToken cancellationToken)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(data);
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, cancellationToken);
        }

        public async Task SendAsync(string data, CancellationToken cancellationToken)
        {
            byte[] bytes = Encoding.GetBytes(data);
            ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);
            await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.Empty, "User close websocket", cancellationToken);
            await CloseSelf(WebSocketCloseStatus.Empty, "User close websocket");
        }

        private async Task CloseSelf(WebSocketCloseStatus status, string desc)
        {
            await OnClose(status, desc);
            Dispose(true);
        }

        protected abstract Task OnBinaryMessage(byte[] data);

        protected abstract Task OnTextMessage(string str);

        protected virtual async Task OnClose(WebSocketCloseStatus status, string desc) {
            await Task.CompletedTask;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    _cancellationTokenSource.Cancel();
                    _receiveTask.Dispose();

                    _webSocket.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~WebSocketClientBase()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
