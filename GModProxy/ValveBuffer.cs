using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSharp
{
    public class ValveBuffer : IDisposable
    {
        public enum ConnectionlessPacketType : byte
        {
            A2S_INFO_REQUEST = 0x54, // T
            A2S_INFO_RESPONSE = 0x49, // I

            A2S_PLAYER_REQUEST = 0x55, // U
            A2S_PLAYER_RESPONSE = 0x44, // D

            connectionrefused = 0x39, // 9

            A2A_GETCHALLENGE = 0x41, //A
            C2S_CONNECT = 0x6b, // k

            S2C_CONNECTION = 0x42, // B

            sendchallange = 0x71, // q
        }


        public const int NET_HEADER_FLAG_QUERY = -1;
        public const int NET_HEADER_FLAG_SPLITPACKET = -2;
        public const int NET_HEADER_FLAG_COMPRESSEDPACKET = -3;



        MemoryStream stream;
        BinaryWriter writer;
        BinaryReader reader;

        public ValveBuffer(byte[] buffer, int offset, int size)
        {
            stream = new MemoryStream(buffer, offset, size);
            reader = new BinaryReader(stream);
        }

        public ValveBuffer()
        {
            stream = new MemoryStream();
            writer = new BinaryWriter(stream);
        }

        public void WriteByte(byte val) => writer.Write(val);
        public byte ReadByte() => reader.ReadByte();

        public void WriteShort(short val) => writer.Write(val);
        public short ReadShort() => reader.ReadInt16();

        public void WriteLong(int val) => writer.Write(val);
        public int ReadLong() => reader.ReadInt32();

        public void WriteFloat(float val) => writer.Write(val);
        public float ReadFloat() => reader.ReadSingle();

        public void WriteLongLong(long val) => writer.Write(val);
        public long ReadLongLong() => reader.ReadInt64();

        public void WriteString(string val)
        {
            writer.Write(Encoding.UTF8.GetBytes(val));
            writer.Write((byte)0x00);
        }

        public string ReadString()
        {
            var bytes = new List<byte>();
            var nextByte = reader.ReadByte();
            while (nextByte != 0x00)
            {
                bytes.Add(nextByte);
                nextByte = reader.ReadByte();
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public byte[] ToArray()
        {
            return stream.ToArray();
        }

        public void Dispose()
        {
            writer.Dispose();
            stream.Dispose();
        }
    }
}
