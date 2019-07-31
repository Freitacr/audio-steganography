using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AudioSteganography.Audio.Flac
{
    public class BitReader
    {
        private Stream BaseStream;
        private byte Buffer = 0;
        public int BitPosition { get; private set; } = -1;
        public BitReader(Stream streamIn)
        {
            BaseStream = streamIn;
        }

        public bool ReadBit()
        {
            if (BitPosition < 0)
            {
                int readByte = BaseStream.ReadByte();
                if (readByte < 0)
                    throw new EndOfStreamException("Attempted to read past the end of the stream");
                Buffer = (byte)readByte;
                BitPosition = 7;
            }
            int mask = 0x1 << BitPosition;
            int res = mask & Buffer;
            bool ret = res != 0;
            Buffer = (byte)(Buffer & ~mask);
            BitPosition--;
            return ret;
        }

        public byte ReadByte()
        {
            byte ret = 0;
            for (int currPos = 7; currPos >= 0; currPos--)
            {
                int val = ReadBit() ? 0x1 : 0x0;
                val <<= currPos;
                ret = (byte)(ret | val);
            }
            return ret;
        }

        /**
         * <summary>Forces the buffer to be cleared by the next read</summary>
         */
        public void ClearBuffer()
        {
            BitPosition = -1;
        }

        /**
         * <summary>Reads a string of unary bits an returns the number of bits it 
         * took to get to a bit with the value of 1</summary>
         */
        public uint ReadUnary()
        {
            uint ret = 0;
            bool currBit = false;
            while (!currBit)
            {
                currBit = ReadBit();
                ret++;
            }
            return ret;
        }

        public uint ReadSpecifiedBitCount(int bitCount)
        {
            uint ret = 0;
            for (int i = bitCount; i > 0; i--)
            {
                uint val = ReadBit() ? 0x1u : 0x0u;
                val <<= (i-1);
                ret |= val;
            }
            return ret;
        }
    }
}
