using System;
using System.Collections.Generic;

namespace GModProxy
{
    class Program
    {

        static ReplyInfoPacket infoPacket = new ReplyInfoPacket
        {
            AmountBots = 0,
            AmountClients = 250,
            Appid = 4000,
            GameDirectory = "garrysmod",
            GamemodeName = "Sandbox",
            ServerName = "this is my server name?",
            GameVersion = ReplyInfoPacket.default_game_version,
            MapName = "gm_fuckmynuts",
            MaxClients = 255,
            OS = ReplyInfoPacket.OSType.Windows,
            Passworded = false,
            Secure = false,
            Server = ReplyInfoPacket.ServerType.Dedicated,
            UDPPort = 27016,
            SteamID = 0,
            Tags = "gm:sandbox"

        };

        static  ReplyPlayerPacket playerPacket = new ReplyPlayerPacket
        {
            Players = new List<ReplyPlayer>
                {
                    new ReplyPlayer{Name = "Duck", Score = 123, Time = 10},
                    new ReplyPlayer{Name = "Fuck", Score = 456, Time = 9},
                    new ReplyPlayer{Name = "Suck", Score = 789, Time = 8},
                }
        };

        static void Main(string[] args)
        {
            Console.WriteLine($"UDP server port: {infoPacket.UDPPort}");

            Console.WriteLine();

            // Create a new UDP Proxy server
            var server = new SRCDSServerProxy(infoPacket.UDPPort, "192.168.0.208", 27015, (e)=>infoPacket, (e) => playerPacket);

            // Start the server
            Console.Write("Server starting...");
            server.Start();
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the server or '!' to restart the server...");

            // Perform text input
            for (; ; )
            {
                string line = Console.ReadLine();
                if (line == string.Empty)
                    break;

                // Restart the server
                if (line == "!")
                {
                    Console.Write("Server restarting...");
                    server.Restart();
                    Console.WriteLine("Done!");
                }
            }

            // Stop the server
            Console.Write("Server stopping...");
            server.Stop();
            Console.WriteLine("Done!");
        }
    }
}
