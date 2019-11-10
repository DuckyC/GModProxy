using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GModProxy
{
    public class SplitPacket
    {
        private readonly MemoryStream memoryStream;

        public SplitPacket()
        {
            memoryStream = new MemoryStream();
        }

        public void AddContent(byte[] buffer, int offset, int size)
        {
            memoryStream.Write(buffer, offset, size);
        }

        public byte[] GetAll()
        {
            return memoryStream.ToArray();
        }
    }
}
