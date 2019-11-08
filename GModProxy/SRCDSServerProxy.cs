using GSharp;
using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GModProxy
{
    public class SRCDSServerProxy : UdpServer
    {
        private readonly string hostIPAddress;
        private readonly int hostPort;
        private readonly Func<EndPoint, ReplyInfoPacket> getInfoPacket;
        private readonly Func<EndPoint, ReplyPlayerPacket> getPlayerPacket;

        private Dictionary<string, SRCDSClient> clients = new Dictionary<string, SRCDSClient>();

        public SRCDSServerProxy(int listenPort, string hostIPAddress, int hostPort, Func<EndPoint, ReplyInfoPacket> getInfoPacket, Func<EndPoint, ReplyPlayerPacket> getPlayerPacket) : base(IPAddress.Any, listenPort)
        {
            this.hostIPAddress = hostIPAddress;
            this.hostPort = hostPort;
            this.getInfoPacket = getInfoPacket;
            this.getPlayerPacket = getPlayerPacket;
        }

        protected override void OnStarted()
        {
            OptionSendBufferSize = 2048;
            OptionReceiveBufferSize = 2048;
            ReceiveAsync();
        }
        protected override void OnSent(EndPoint endpoint, long sent) => ReceiveAsync();
        protected override void OnError(SocketError error) => Console.WriteLine($"Proxy UDP server caught an error with code {error}");

        protected override void OnReceived(EndPoint endpoint, byte[] rawBuffer, long offset, long size)
        {
            var buffer = new ValveBuffer(rawBuffer, (int)offset, (int)size);
            var header = buffer.ReadLong();

            if (header == ValveBuffer.NET_HEADER_FLAG_QUERY)
            {
                switch (rawBuffer[4])
                {
                    case ReplyInfoPacket.RequestType:
                        var infoBytes = getInfoPacket(endpoint).GetPacket();
                        Console.WriteLine("Sending fake A2S_INFO packet");
                        SendAsync(endpoint, infoBytes, 0, infoBytes.Length);
                        return;
                    case ReplyPlayerPacket.RequestType:
                        var playerBytes = getPlayerPacket(endpoint).GetPacket();
                        Console.WriteLine("Sending fake A2S_PLAYER packet");
                        SendAsync(endpoint, playerBytes, 0, playerBytes.Length);
                        return;
                }
            }

            var key = endpoint.ToString();
            if (!clients.TryGetValue(key, out SRCDSClient client))
            {
                client = new SRCDSClient(hostIPAddress, hostPort, (buffer, offset, size) =>
                {
                    Console.WriteLine("S2C=>" + BitConverter.ToString(buffer, (int)offset, (int)size));
                    SendAsync(endpoint, buffer, offset, size);
                });

                client.Connect();
                clients.Add(key, client);
            }

            Console.WriteLine("C2S=>" + BitConverter.ToString(rawBuffer, (int)offset, (int)size));
            client.SendAsync(rawBuffer, offset, size);
        }
    }
}
