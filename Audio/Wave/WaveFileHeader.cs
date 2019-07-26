using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using AudioSteganography.Helper;

namespace AudioSteganography.Audio.Wave
{
    public class ImproperFormatException : Exception
    {
        public static readonly string DEFAULT_MESSAGE = "WAVE file was in improper format...";
        public ImproperFormatException() : base(DEFAULT_MESSAGE) { }
        public ImproperFormatException(string message) : base(message) { }
    }
    
    public class WaveFileHeader
    {
        private string ChunkId_;
        public string ChunkId { get { return ChunkId_; } }
        private int ChunkSize_;
        public int ChunkSize { get { return ChunkSize_; } }
        private string ChunkFormat_;
        public string ChunkFormat { get { return ChunkFormat_; } }
        private string FmtChunkFormat_;
        public string FmtChunkFormat { get { return FmtChunkFormat_; } }

        private Stream FileInputStream;
        


        public WaveFileHeader(Stream fileInputStream)
        {
            FileInputStream = fileInputStream;
        }

        public void ParseWaveHeader()
        {
            ParseRiffChunkHeader();
        }

        private void ParseChunkId()
        {
            byte[] fieldBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                int readByte = FileInputStream.ReadByte();
                if (readByte == -1)
                    throw new ImproperFormatException("WAVE file did not contain a full header");
                fieldBytes[i] = (byte)readByte;
            }
            byte[] expectedValue = new byte[] { 0x52, 0x49, 0x46, 0x46 }; //RIFF
            for (int i = 0; i < 4; i++)
                if (!(fieldBytes[i] == expectedValue[i]))
                    throw new ImproperFormatException("WAVE file did not begin with a RIFF chunk");
            ChunkId_ = "RIFF";
        }

        private void ParseChunkSize()
        {
            byte[] fieldBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                int readByte = FileInputStream.ReadByte();
                if (readByte == -1)
                    throw new ImproperFormatException("WAVE file did not contain a full header");
                fieldBytes[i] = ByteHelper.ReverseEndian((byte)readByte);
            }
            ChunkSize_ = ByteHelper.ReverseEndian(ByteHelper.BytesToInt(fieldBytes));
        }

        private void ParseChunkFormat()
        {
            byte[] fieldBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                int readByte = FileInputStream.ReadByte();
                if (readByte == -1)
                    throw new ImproperFormatException("WAVE file did not contain a full header");
                fieldBytes[i] = (byte)readByte;
            }
            byte[] expectedValue = new byte[] { 0x57, 0x41, 0x56, 0x45 }; //WAVE
            for (int i = 0; i < 4; i++)
                if (!(fieldBytes[i] == expectedValue[i]))
                    throw new ImproperFormatException("WAVE file did not have a WAVE chunk");
            ChunkId_ = "WAVE";
        }

        private void ParseRiffChunkHeader()
        {
            ParseChunkId();
            ParseChunkSize();
            ParseChunkFormat();
        }
    }
}
