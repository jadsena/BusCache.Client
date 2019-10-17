using System;

namespace BusCache.Client
{
    public interface IConnect : IDisposable
    {
        event EventHandler<string> Receive;
        event EventHandler ConnectionClose;
        void SetKey(string key, string value);
        void GetKey(string key);
        void SendDataToService(string serviceName, string Data);
    }
}
