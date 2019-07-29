using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AudioSteganography.Helper;

namespace AudioSteganography.Audio.Flac
{
    public class MD5Signature
    {
        public byte[] Signature = new byte[16];

        public MD5Signature() { }


    }

    public class StreamInfo : MetadataBlock
    {

        public short MinimumBlockSize { get; private set; }
        public short MaximumBlockSize { get; private set; }
        public int MinimumFrameSize { get; private set; }
        public int MaximumFrameSize { get; private set; }
        public int SampleRateHz { get; private set; }
        public int NumberChannels { get; private set; }
        public int BitsPerSample { get; private set; }
        public long TotalSamples { get; private set; }
        public MD5Signature MD5Signature { get; private set; }

        public StreamInfo(MetadataBlock baseBlock) : base()
        {
            Data = baseBlock.Data;
            Length = baseBlock.Length;
            IsLast = baseBlock.IsLast;
            MetadataType = MetadataBlock.BLOCK_TYPE.STREAMINFO;
            ParseSelfFromData();
        }

        private void ParseSelfFromData()
        {
            ParseMinBlockSize();
            ParseMaxBlockSize();
            ParseMinimumFramesize();
            ParseMaximumFramesize();
            ParseSampleRate();
            ParseChannels();
            ParseSampleBits();
            ParseTotalSamples();
            ParseMD5Signature();
        }

        private void ParseMinBlockSize()
        {
            byte[] sizeBytes = new byte[] { Data[0], Data[1] };
            MinimumBlockSize = (short)ByteHelper.BytesToShort(sizeBytes);
            if ((MinimumBlockSize & 0x0f) != 0)
                throw new ImproperFormatException("Minimum block size used invalid bits");
        }

        private void ParseMaxBlockSize()
        {
            byte[] sizeBytes = new byte[] { Data[2], Data[3] };
            MaximumBlockSize = (short)ByteHelper.BytesToShort(sizeBytes);
            if ((MaximumBlockSize & 0x0f) != 0)
                throw new ImproperFormatException("Maximum block size used invalid bits");
        }

        private void ParseMinimumFramesize()
        {
            byte[] sizeBytes = new byte[] { Data[4], Data[5], Data[6], 0x00 };
            MinimumFrameSize = ByteHelper.BytesToInt(sizeBytes);
        }

        private void ParseMaximumFramesize()
        {
            byte[] sizeBytes = new byte[] { Data[7], Data[8], Data[9], 0x00 };
            MaximumFrameSize = ByteHelper.BytesToInt(sizeBytes);
        }

        private void ParseSampleRate()
        {
            byte[] rateBytes = new byte[] {
                0x00,
                (byte)(Data[10] >> 4),
                (byte)((Data[10] << 4) | ((Data[11] & 0xF0) >> 4)),
                (byte)((Data[11] << 4) | ((Data[12] & 0xF0) >> 4))};
            SampleRateHz = ByteHelper.BytesToInt(rateBytes);
        }

        private void ParseChannels()
        {
            byte toParse = (byte)((Data[12] & 0x0e) >> 1); //1101
            NumberChannels = toParse + 1;
        }

        private void ParseSampleBits()
        {
            byte toParse = (byte)(
                ((Data[12] & 0x01) << 4) |
                ((Data[13] & 0xf0) >> 4)
            );
            BitsPerSample = toParse + 1;
        }

        private void ParseTotalSamples()
        {
            byte[] sampleBytes = new byte[]
            {
                0x00, 0x00, 0x00, (byte)(Data[13] & 0x0f),
                Data[14], Data[15], Data[16], Data[17]
            };
            TotalSamples = ByteHelper.BytesToLong(sampleBytes);
        }

        private void ParseMD5Signature()
        {
            MD5Signature = new MD5Signature();
            for (int i = 0; i < 16; i++)
                MD5Signature.Signature[i] = Data[18 + i];
        }
    }
}
