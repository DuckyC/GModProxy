using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GModProxy
{

    public class SRCDSClient : UdpClient
    {
        private readonly Action<byte[], long, long> sendAsync;

        public SRCDSClient(string address, int port, Action<byte[], long, long> onReceived) : base(address, port)
        {
            this.sendAsync = onReceived;
        }



        protected override void OnConnected()
        {
            Console.WriteLine($"Proxy UDP client connected a new session with Id {Id}");
            OptionReceiveBufferSize = 2048;
            OptionSendBufferSize = 2048;
            // Start receive datagrams
            ReceiveAsync();
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Proxy UDP client disconnected a session with Id {Id}");
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            sendAsync(buffer, offset, size);
            // Continue receive datagrams
            ReceiveAsync();
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            Console.WriteLine($"Proxy UDP client caught an error with code {error}");
        }

    }
}
