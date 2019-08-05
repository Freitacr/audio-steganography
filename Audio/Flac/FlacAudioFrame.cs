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

    public enum CHANNEL_ASSIGNMENT
    {
        /**
         * <summary>If this is the CHANNEL_ASSIGNMENT, then the value of ChannelAssignment in a FlacAudioFrame is the number of channels</summary>
         */
        SMPTE_CHANNEL_RECOMMENDATION,
        LEFT_SIDE_STEREO,
        RIGHT_SIDE_STEREO,
        MID_SIDE_STEREO
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
        public POSITION_VALUE PositionValueType { get; private set; }
        public CHANNEL_ASSIGNMENT ChannelAssignmentType { get; private set; }
        public byte CRC { get; private set; }
        public FlacAudioFrameFooter Footer { get; private set; }
        public FlacSubFrame[] SubFrames { get; private set; }
        private FlacDecoderStream ParentStream;

        public FlacAudioFrame(FlacDecoderStream parentStream)
        {
            ParentStream = parentStream;
        }

        public void ParseAudioFrame(Stream streamIn)
        {
            ParseHeader(streamIn);
            SubFrames = new FlacSubFrame[ParentStream.StreamInfoBlock.NumberChannels];
            BitReader streamReader = new BitReader(streamIn);
            int frameSampleSizeSaved = SampleSize;
            for (int i = 0; i < SubFrames.Length; i++)
            {
                switch(ChannelAssignmentType)
                {
                    case CHANNEL_ASSIGNMENT.LEFT_SIDE_STEREO:
                        if (i == 1)
                            SampleSize++;
                        break;
                    case CHANNEL_ASSIGNMENT.RIGHT_SIDE_STEREO:
                        if (i == 0)
                            SampleSize++;
                        break;
                    case CHANNEL_ASSIGNMENT.MID_SIDE_STEREO:
                        if (i == 1)
                            SampleSize++;
                        break;
                    default: break;
                }

                SubFrames[i] = FlacSubFrame.ReadSubFrame(streamReader, this);
                SampleSize = frameSampleSizeSaved;
            }
            streamReader.ClearBuffer();
            Footer = new FlacAudioFrameFooter((short)streamReader.ReadSpecifiedBitCount(16));
            Console.WriteLine("Finished Processing of frame: " + PositionValue);
        }

        
        private void ParseHeader(Stream streamIn)
        {
            ParseSyncCodeAndBlockingStrategy(streamIn);
            ParseBlockSizeAndSampleRate(streamIn);
            ParseChannelAssignmentAndSampleSize(streamIn);
            ParseSampleOrFrameNumber(streamIn);
            UpdateBlockSizeToTrueValue(streamIn);
            UpdateSampleRateToTrueValue(streamIn);
            UpdateChannelAssignmentToTrueValue();
            UpdateSampleSizeToTrueValue();
            ReadCRC(streamIn);
        }

        private void ParseSyncCodeAndBlockingStrategy(Stream streamIn)
        {
            byte[] readBytes = new byte[2];
            if (streamIn.Read(readBytes, 0, 2) == 0)
                throw new EndOfStreamException();
            ushort headerVal = ByteHelper.BytesToShort(readBytes);
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
            for (int i = parsedVector.Length - 1; i > 0; i--)
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
            switch (InterChannelSampleBlockSize)
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
                    InterChannelSampleBlockSize = (256) * (int)(Math.Pow(2, InterChannelSampleBlockSize - 8));
                    break;
            }
        }

        private void UpdateSampleRateToTrueValue(Stream streamIn)
        {
            switch (SampleRate)
            {
                case 0:
                    List<MetadataBlock>.Enumerator metadataEnum = ParentStream.GetMetadataEnumerator();
                    metadataEnum.MoveNext();
                    SampleRate = (metadataEnum.Current as StreamInfo).SampleRateHz;
                    break;
                case 1:
                    SampleRate = 88200;
                    break;
                case 2:
                    SampleRate = 176400;
                    break;
                case 3:
                    SampleRate = 192000;
                    break;
                case 4:
                    SampleRate = 8000;
                    break;
                case 5:
                    SampleRate = 16000;
                    break;
                case 6:
                    SampleRate = 22050;
                    break;
                case 7:
                    SampleRate = 24000;
                    break;
                case 8:
                    SampleRate = 32000;
                    break;
                case 9:
                    SampleRate = 44100;
                    break;
                case 10:
                    SampleRate = 48000;
                    break;
                case 11:
                    SampleRate = 96000;
                    break;
                case 12:
                    byte sampleRate = (byte)streamIn.ReadByte();
                    SampleRate = sampleRate * 1000;
                    break;
                case 13:
                    byte[] sampleRateBytes = new byte[2];
                    streamIn.Read(sampleRateBytes, 0, 2);
                    SampleRate = ByteHelper.BytesToShort(sampleRateBytes);
                    break;
                case 14:
                    byte[] sampleRateHzBytes = new byte[2];
                    streamIn.Read(sampleRateHzBytes, 0, 2);
                    SampleRate = ByteHelper.BytesToShort(sampleRateHzBytes) * 10;
                    break;
                default:
                    throw new ImproperFormatException("Sample Rate was invalid");
            }
        }

        private void UpdateChannelAssignmentToTrueValue()
        {
            switch (ChannelAssignment)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    ChannelAssignmentType = CHANNEL_ASSIGNMENT.SMPTE_CHANNEL_RECOMMENDATION;
                    ChannelAssignment++;
                    break;
                case 8:
                    ChannelAssignmentType = CHANNEL_ASSIGNMENT.LEFT_SIDE_STEREO;
                    break;
                case 9:
                    ChannelAssignmentType = CHANNEL_ASSIGNMENT.RIGHT_SIDE_STEREO;
                    break;
                case 10:
                    ChannelAssignmentType = CHANNEL_ASSIGNMENT.MID_SIDE_STEREO;
                    break;
                default:
                    throw new ImproperFormatException("Reserved value found for channel assignment value");
            }
        }

        private void UpdateSampleSizeToTrueValue()
        {
            switch (SampleSize)
            {
                case 0:
                    var enumerator = ParentStream.GetMetadataEnumerator();
                    enumerator.MoveNext();
                    SampleSize = (enumerator.Current as StreamInfo).BitsPerSample;
                    break;
                case 1:
                    SampleSize = 8;
                    break;
                case 2:
                    SampleSize = 12;
                    break;
                case 4:
                    SampleSize = 16;
                    break;
                case 5:
                    SampleSize = 20;
                    break;
                case 6:
                    SampleSize = 24;
                    break;
                default:
                    throw new ImproperFormatException("Sample size was a reserved value and is invalid");
            }
        }

        private void ReadCRC(Stream streamIn)
        {
            CRC = (byte)streamIn.ReadByte();
        }
    }

}
