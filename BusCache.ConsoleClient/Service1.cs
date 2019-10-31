using BusCache.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusCache.ConsoleClient
{
    public class Service1 : IDisposable
    {
        private readonly IConnect _connect;
        public event EventHandler<string> Receive;
        public Service1(IConnect connect)
        {
            _connect = connect;
            _connect.Receive += Connect_Receive;
        }

        private void Connect_Receive(object sender, string e)
        {
            Receive?.Invoke(this, e);
        }

        public void Set(string key, string value)
        {
            _connect.SetKey(key, value);
        }
        public void Get(string key)
        {
            _connect.GetKey(key);
        }
        public void Execute(string command)
        {
            _connect.Execute(command);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                _connect.Dispose();
            }
        }
    }
}
