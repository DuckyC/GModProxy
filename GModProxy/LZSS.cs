using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GModProxy
{
    public static class LZSS
    {
        public const UInt32 LZSS_ID = ('S' << 24) | ('S' << 16) | ('Z' << 8) | ('L');
        public const int LZSS_LOOKSHIFT = 4;
        public static int Decompress(byte[] inputBuffer, int inputSize, out byte[] outputBuffer)
        {
            uint id;
            uint actualSize;

            using (var reader = new BinaryReader(new MemoryStream(inputBuffer)))
            {
                id = reader.ReadUInt32();
                actualSize = reader.ReadUInt32();
            }

            if (id != LZSS_ID)
            {
                outputBuffer = inputBuffer;
                return inputSize;
            }
            else
            {
                var totalBytes = 0;
                outputBuffer = new byte[actualSize];
                var inputPosition = 0;
                var outputPosition = 0;
                int cmdByte = 0;
                int getCmdByte = 0;
                while (true)
                {
                    if (getCmdByte == 0)
                    {
                        cmdByte = inputBuffer[inputPosition++];
                    }
                    getCmdByte = (getCmdByte + 1) & 0x07;
                    if ((cmdByte & 0x01) == 1)
                    {
                        var currentByte = inputBuffer[inputPosition++];
                        var position = currentByte << LZSS_LOOKSHIFT;
                        position |= currentByte >> LZSS_LOOKSHIFT;

                        int count = (inputBuffer[inputPosition++] & 0x0F) + 1;
                        if (count == 1)
                        {
                            break;
                        }

                        var sourcePosition = outputPosition - position - 1;
                        for (int i = 0; i < count; i++)
                        {
                            outputBuffer[outputPosition++] = outputBuffer[sourcePosition++];

                        }
                        totalBytes += count;
                    }
                    else
                    {
                        outputBuffer[outputPosition++] = inputBuffer[inputPosition++];
                        totalBytes++;
                    }
                }

                if (totalBytes != actualSize)
                {
                    throw new Exception("totalBytes and actualSize are not the same!!");
                }
                return totalBytes;
            }
        }
    }
}