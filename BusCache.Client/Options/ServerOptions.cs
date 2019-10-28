using System;
using System.Collections.Generic;
using System.Text;

namespace BusCache.Client.Options
{
    public class ServerOptions
    {
        public string IP { get; set; }
        public int Port { get; set; }
        public string ServiceName { get; set; }
        /// <summary>
        /// TimeoutSendComand in milliseconds
        /// </summary>
        public int TimeoutSendComand { get; set; }
    }
}
