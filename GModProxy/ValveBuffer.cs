using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSharp
{
    public class ValveBuffer : IDisposable
    {
        MemoryStream stream;
        BinaryWriter writer;
        BinaryReader reader;

        public ValveBuffer(byte[] buffer)
        {
            stream = new MemoryStream(buffer);
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
            while(nextByte != 0x00)
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
