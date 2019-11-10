using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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
            if (buffer.ReadHeaderFlag() == NET_HEADER_FLAG.QUERY)
            {
                switch (buffer.ReadConnectionlessPacketType())
                {
                    case ConnectionlessPacketType.A2S_INFO_REQUEST:
                        var infoBytes = getInfoPacket(endpoint).GetPacket();
                        Console.WriteLine("Sending fake A2S_INFO packet");
                        SendAsync(endpoint, infoBytes, 0, infoBytes.Length);
                        return;
                    case ConnectionlessPacketType.A2S_PLAYER_REQUEST:
                        var playerBytes = getPlayerPacket(endpoint).GetPacket();
                        Console.WriteLine("Sending fake A2S_PLAYER packet");
                        SendAsync(endpoint, playerBytes, 0, playerBytes.Length);
                        return;
                }
            }

            var key = endpoint.ToString();
            if (!clients.TryGetValue(key, out SRCDSClient client))
            {
                client = new SRCDSClient(hostIPAddress, hostPort, (rawBuffer, offset, size) =>
                {
                    var temp = new ValveBuffer(rawBuffer, (int)offset, (int)size);
                    var header = temp.ReadHeaderFlag();

                    if (header == NET_HEADER_FLAG.SPLITPACKET)
                    {
                        var netid = temp.ReadLong();
                        var sequenceNumber = temp.ReadLong();
                        var packetIDAndSplitSize = temp.ReadLong();

                        var packetID = (packetIDAndSplitSize & 0xFFFF0000) >> 4;
                        var splitSize = (packetIDAndSplitSize & 0x0000FFFF);

                        Console.WriteLine("S2C=>Splitpacket");
                        SendAsync(endpoint, rawBuffer, offset, size);
                        return;
                    }

                    var parsingBuffer = rawBuffer;
                    if(header == NET_HEADER_FLAG.COMPRESSEDPACKET)
                    {
                        size = LZSS.Decompress(rawBuffer, (int)size, out byte[] outputBuffer);
                        parsingBuffer = outputBuffer;
                    }

                    var parsed = ValvePacketHandler.HandlePacket(parsingBuffer, offset, size);
                    Console.WriteLine("S2C=>" + (parsed ?? "Other"));


                    SendAsync(endpoint, rawBuffer, offset, size);
                });

                client.Connect();
                clients.Add(key, client);
            }

            var parsed = ValvePacketHandler.HandlePacket(rawBuffer, offset, size);
            Console.WriteLine("C2S=>" + (parsed ?? BitConverter.ToString(rawBuffer, (int)offset, (int)size)));
            client.SendAsync(rawBuffer, offset, size);
        }
    }
}
