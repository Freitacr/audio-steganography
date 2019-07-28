using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using AudioSteganography.Helper;

namespace AudioSteganography.Audio.Flac
{
    public class FlacAudioFrameFooter
    {
        public short CRC { get; private set; }
        public FlacAudioFrameFooter(short crc)
        {
            CRC = crc;
        }
    }

    public enum POSITION_VALUE
    {
        SAMPLE_NUMBER,
        FRAME_NUMBER
    }

    public class FlacAudioFrame
    {
        private short SyncCode;
        public bool IsBlocksizeVariable { get; private set; }
        public int InterChannelSampleBlockSize { get; private set; }
        public int SampleRate { get; private set; }
        public int ChannelAssignment { get; private set; }
        public int SampleSize { get; private set; }
        public long PositionValue { get; private set; }
        public POSITION_VALUE PositionValueType {get; private set;}
        public byte CRC { get; private set; }
        public FlacAudioFrameFooter Footer { get; private set; }
        private FlacDecoderStream ParentStream;

        public FlacAudioFrame(FlacDecoderStream parentStream)
        {
            ParentStream = parentStream;
        }

        public void ParseAudioFrame(Stream streamIn)
        {
            ParseHeader(streamIn);
        }

        private void ParseHeader(Stream streamIn)
        {
            ParseSyncCodeAndBlockingStrategy(streamIn);
            ParseBlockSizeAndSampleRate(streamIn);
            ParseChannelAssignmentAndSampleSize(streamIn);
            ParseSampleOrFrameNumber(streamIn);
            UpdateBlockSizeToTrueValue(streamIn);
        }

        private void ParseSyncCodeAndBlockingStrategy(Stream streamIn)
        {
            byte[] readBytes = new byte[2];
            streamIn.Read(readBytes, 0, 2);
            short headerVal = ByteHelper.BytesToShort(readBytes);
            if ((headerVal ^ 0xfff8) != 0)
                throw new ImproperFormatException("Audio Frame Sync Code was incorrect");
            SyncCode = (short)(headerVal & 0xfff8);
            IsBlocksizeVariable = (headerVal & 0x1) == 1;
        }

        private void ParseBlockSizeAndSampleRate(Stream streamIn)
        {
            byte readByte = (byte)streamIn.ReadByte();
            InterChannelSampleBlockSize = (readByte & 0xF0) >> 4;
            SampleRate = readByte & 0x0F;
        }

        private void ParseChannelAssignmentAndSampleSize(Stream streamIn)
        {
            byte readByte = (byte)streamIn.ReadByte();
            ChannelAssignment = (readByte & 0xF0) >> 4;
            SampleSize = (readByte & 0x0E) >> 1;
        }

        private byte[] ParseUTF8StyleVector(Stream streamIn)
        {
            List<byte> ret = new List<byte>();
            byte markerByte = (byte)streamIn.ReadByte();
            int mask = 0x80;
            int bytesToRead = 0;
            ret.Add(markerByte);
            for (int i = 0; i < 7; i++)
            {
                int currMask = mask >> i;
                if ((currMask & markerByte) != 0)
                    bytesToRead += 1;
                else break;
            }
            bytesToRead = bytesToRead == 0 ? bytesToRead : bytesToRead - 1;
            for (int i = 1; i < bytesToRead; i++)
                ret.Add((byte)streamIn.ReadByte());
            return ret.ToArray();
        }

        private long ExtractLongFromVector(byte[] parsedVector)
        {
            long ret = 0;
            int utfMask = 0x3f;
            int longPosition = 0;
            for (int i = parsedVector.Length-1; i > 0; i--)
            {
                int res = parsedVector[i] & utfMask;
                res = res << longPosition;
                longPosition += 6;
                ret |= (long)res;
            }
            int usableBits = 8 - (parsedVector.Length + 1);
            usableBits = parsedVector.Length == 1 ? 7 : usableBits;
            int usableBitMask = 0x0;
            for (int i = 0; i < usableBits; i++)
                usableBitMask = (usableBitMask << 1) | 1;
            int topLongBits = parsedVector[0] & usableBitMask;
            topLongBits = topLongBits << longPosition;
            ret |= (long)topLongBits;
            return ret;
        }

        private void ParseSampleOrFrameNumber(Stream streamIn)
        {
            byte[] val = ParseUTF8StyleVector(streamIn);
            PositionValue = ExtractLongFromVector(val);
            PositionValueType = IsBlocksizeVariable ? POSITION_VALUE.SAMPLE_NUMBER : POSITION_VALUE.FRAME_NUMBER;
        }

        private void UpdateBlockSizeToTrueValue(Stream streamIn)
        {
            switch(InterChannelSampleBlockSize)
            {
                case 0:
                    throw new ImproperFormatException("Parsed block size of 0 is reserved");
                case 1:
                    InterChannelSampleBlockSize = 192;
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                    InterChannelSampleBlockSize = (576) * (int)Math.Pow(2, InterChannelSampleBlockSize - 2);
                    break;
                case 6:
                    byte readByte = (byte)streamIn.ReadByte();
                    InterChannelSampleBlockSize = (int)readByte;
                    InterChannelSampleBlockSize++;
                    break;
                case 7:
                    byte[] readBytes = new byte[2];
                    streamIn.Read(readBytes, 0, 2);
                    InterChannelSampleBlockSize = ByteHelper.BytesToShort(readBytes);
                    InterChannelSampleBlockSize++;
                    break;
                default:
                    InterChannelSampleBlockSize = (576) * (int)(Math.Pow(2, InterChannelSampleBlockSize - 8));
                    break;
            }
        }

        private void UpdateSampleRateToTrueValue(Stream streamIn)
        {
            switch(SampleRate)
            {

            }
        }

        private void ParseDefinedBlockSizeAndSampleRate(Stream streamIn)
        {
            if (InterChannelSampleBlockSize == 6)
            {
                
            } else if (InterChannelSampleBlockSize == 7)
            {
                
            }
            if (SampleRate == 12)
            {

            } else if (SampleRate == 13)
            {

            } else if (SampleRate == 14)
            {

            }
        }
    }

}
