using ExchangeApi.Core;
using System;
using System.Security.Cryptography;
using System.Text;

namespace ExchangeApi.Huobi.Core
{
    public class HuobiSigner : ISigner
    {
        private static Encoding encoding = Encoding.UTF8;

        HMACSHA256 _hmacsha256;

        public HuobiSigner(string key)
        {
            byte[] keyBuffer = encoding.GetBytes(key);
            _hmacsha256 = new HMACSHA256(keyBuffer);
        }

        public string Sign(string method, string host, string path, string parameters)
        {
            if (string.IsNullOrEmpty(method)
                || string.IsNullOrEmpty(host)
                || string.IsNullOrEmpty(path)
                || string.IsNullOrEmpty(parameters))
                return string.Empty;

            string str = $"{method}\n{host}\n{path}\n{parameters}";
            return Sign(str);
        }

        public string Sign(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            byte[] inputBuffer = encoding.GetBytes(input);

            byte[] hashedBuffer = _hmacsha256.ComputeHash(inputBuffer);

            return Convert.ToBase64String(hashedBuffer);
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
                    _hmacsha256.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~HuobiSigner()
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
