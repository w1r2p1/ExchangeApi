using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ExchangeApi.Core
{
    public interface ISigner : IDisposable {
        string Sign(string method, string host, string path, string parameters);

        string Sign(string input);
    }
}
