using System;
using System.Collections.Generic;
using System.Text;

namespace GModProxy
{

    public class ValvePacketHandler
    {
        [Flags]
        public enum PACKET_FLAG : int
        {

            RELIABLE = (1 << 0),// packet contains subchannel stream data
            COMPRESSED = (1 << 1),// packet is compressed
            ENCRYPTED = (1 << 2),// packet is encrypted
            SPLIT = (1 << 3),// packet is split
            CHOKED = (1 << 4),// packet was choked by sender
            CHALLENGE = (1 << 5),// packet is a challenge
            UNKNOWN1 = (1 << 6), // IDK, but its there?
            UNKNOWN2 = (1 << 7), // IDK, but its there?
            TABLES = (1 << 10), //custom flag, request string tables

        }


        public static string HandlePacket(byte[] rawBuffer, long offset, long size)
        {
            var buffer = new ValveBuffer(rawBuffer, (int)offset, (int)size);
            var flag = buffer.ReadHeaderFlag();

            switch (flag)
            {
                case NET_HEADER_FLAG.QUERY:
                    var type = buffer.ReadConnectionlessPacketType();
                    switch (type)
                    {
                        case ConnectionlessPacketType.connectionrefused:
                            return ValvePackets.Read_S2C_ConnectionRefused(buffer);
                        case ConnectionlessPacketType.A2A_GETCHALLENGE:
                            return ValvePackets.Read_A2A_GETCHALLENGE(buffer);
                        case ConnectionlessPacketType.C2S_CONNECT:
                            return ValvePackets.Read_C2S_CONNECT(buffer);
                        case ConnectionlessPacketType.S2C_CONNECTION:
                            return ValvePackets.Read_S2C_CONNECTION(buffer);
                        case ConnectionlessPacketType.sendchallange:
                            return "Send challange packet";
                        default:
                            return "Unknown connectionless packet type";
                    }
                case NET_HEADER_FLAG.SPLITPACKET:
                    return "Split packet";
                case NET_HEADER_FLAG.COMPRESSEDPACKET:
                    return "Compressed packet????";
            }

            buffer.Reset();

            int sequence = buffer.ReadLong();
            int sequence_ack = buffer.ReadLong();
            var flags = (PACKET_FLAG)buffer.ReadByte();
            ushort usCheckSum = (ushort)buffer.ReadShort();

            //if(buffer.GetNumBitsRead() % 8 != 0) { /*Something is wrong*/  }

            int reliableState = buffer.ReadByte();
            int nChoked = 0;

            if (flags.HasFlag(PACKET_FLAG.CHOKED))
            {
                nChoked = buffer.ReadByte();
            }

            if (flags.HasFlag(PACKET_FLAG.CHALLENGE))
            {

            }

            if (sequence == 0x36)
                flags |= PACKET_FLAG.TABLES;

            return "Flags: " + flags.ToString();
        }
    }

    public class ValvePackets
    {
        public const int STEAM_KEYSIZE = 2048;  // max size needed to contain a steam authentication key (both server and client)

        public static string Read_S2C_ConnectionRefused(ValveBuffer buffer)
        {
            var unknown = buffer.ReadLong();
            var errorString = buffer.ReadString();
            return "Connection Refused: " + errorString;
        }

        public static string Read_S2C_CONNECTION(ValveBuffer buffer)
        {

            return "S2C_CONNECTION";
        }


        public static string Read_C2S_CONNECT(ValveBuffer buffer)
        {
            var protocolVersion = buffer.ReadLong();
            var authProtocol = buffer.ReadLong();//auth protocol 0x03 = PROTOCOL_STEAM, 0x02 = PROTOCOL_HASHEDCDKEY, 0x01=PROTOCOL_AUTHCERTIFICATE
            var serverChallange = buffer.ReadLong();
            var clientChallange = buffer.ReadLong();

            var unknown = (ulong)buffer.ReadLong();

            var username = buffer.ReadString();
            var password = buffer.ReadString();
            var gameVersion = buffer.ReadString();

            var unknown2 = buffer.ReadShort();
            var steamID64 = (ulong)buffer.ReadLongLong();

            var steamkey = buffer.ReadBytes(STEAM_KEYSIZE);

            return string.Format("C2S_CONNECT - Protocol {0:X} - AuthProtocol {1:X} - ServerChallange {2} - ClientChallange {3} - UnknownUBitLong - {4} - Username {5} - Password {6} - GameVersion {7} - Unknown2 {8} - SteamID64 {9} - SteamKeySize {10}", protocolVersion, authProtocol, serverChallange, clientChallange, unknown, username, password, gameVersion, unknown2, steamID64, steamkey.Length);
        }

        public static string Read_A2A_GETCHALLENGE(ValveBuffer buffer)
        {
            var magicNumber = buffer.ReadLong();
            var serverChallange = buffer.ReadLong();
            var clientChallange = buffer.ReadLong();
            var authProtocol = buffer.ReadLong();

            var steamkey_encryptionsize = buffer.ReadShort();
            var steamKey = buffer.ReadBytes(steamkey_encryptionsize);

            //var serverSteamID = buffer.ReadBytes(STEAM_KEYSIZE);
            var vacEnabled = 0; buffer.ReadByte();

            return string.Format("A2A_GETCHALLANGE - MagicNumber: {0} - ServerChallange {1} - ClientChallange {2} - AuthProtocol {3:X} - SteamKeySize: {4} - VAC: {5} ", magicNumber, serverChallange, clientChallange, authProtocol, steamkey_encryptionsize, vacEnabled == 1);
        }
    }


}
