using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using AudioSteganography.Audio;
using AudioSteganography.Helper;
using FlaCdotNet.Metadata;
using System.Text;


namespace AudioSteganography
{
    namespace Steganography
    {
        class AudioByteDataBitAssociation
        {
            public int AudioByte;
            public int BitPosition;
            public bool Modified = false;
            public AudioByteDataBitAssociation(int audioByte, int bitPosition)
            {
                AudioByte = audioByte;
                BitPosition = bitPosition;
            }

        }

        class DataByteMapping
        {
            public byte DataFileByte;
            public uint[] MappedBytes = new uint[8];
            public DataByteMapping(byte dataByteIn)
            {
                DataFileByte = dataByteIn;
            }

            public ByteBitMapping TryGetByteMapping(uint byteIn, uint defaultVal)
            {
                for (int i = 0; i < MappedBytes.Length; i++)
                {
                    if (MappedBytes[i] == byteIn)
                    {
                        return new ByteBitMapping(byteIn, i);
                    }
                }
                return new ByteBitMapping(defaultVal, -1);
            }
        }

        class ByteBitMapping
        {
            public uint MappedByte;
            public int BitPosition;
            public ByteBitMapping(uint mappedByte, int bitPosition)
            {
                MappedByte = mappedByte;
                BitPosition = bitPosition;
            }
        }

        class ByteBitAssociationComparer : IComparer<AudioByteDataBitAssociation>
        {
            public int Compare(AudioByteDataBitAssociation x, AudioByteDataBitAssociation y)
            {
                return x.BitPosition.CompareTo(y.BitPosition);
            }
        }

        public class NLSBSteganographerEncoder
        {
            private FileInDecodingStream AudioFileStream;
            private Stream DataFileStream;
            private KeyedRng Rng;
            public Action<int[]> WriteCallback = null;
            private Dictionary<uint, DataByteMapping> PositionMapping = new Dictionary<uint, DataByteMapping>();
            private uint AudioTotalSamples { get { return (uint)(AudioFileStream.GetTotalSamples() * AudioFileStream.GetChannels()); } }
            private uint TotalReadSamples = 0;

            /**
             * <summary>audioFileStream must be initialized prior to passing
             * dataFileStream must be open prior to passing</summary>
             */
            public NLSBSteganographerEncoder(FileInDecodingStream audioFileStream, Stream dataFileStream, byte[] key, int dataFileSize)
            {
                if ((audioFileStream.GetTotalSamples()*audioFileStream.GetChannels()) < ((uint)dataFileSize * 8))
                    throw new InsufficientAudioException();
                AudioFileStream = audioFileStream;
                DataFileStream = dataFileStream;
                Rng = new KeyedRng(key);
            }

            public void Clear()
            {
                PositionMapping.Clear();
            }

            /**
             * <summary>Extracts the bit from the byte supplied.
             * bitPosition is a zero-indexed number in the range 0-7, and refers to the bit <code>bitPosition</code> from the right</summary>
             */
            private bool ExtractBitFromByte(int b, int bitPosition)
            {
                int mask = 1;
                int res = b & (mask << bitPosition);
                return res != 0;
            }

            public void ProcessSampleBlock(int[] samplesIn)
            {
                int[] samplesOut = new int[samplesIn.Length];
                for (int i = 0; i < samplesIn.Length; i++)
                {
                    if (PositionMapping.TryGetValue((uint)(i + TotalReadSamples), out DataByteMapping mappedVal))
                    {
                        var byteMapping = mappedVal.TryGetByteMapping((uint)(i + TotalReadSamples), AudioTotalSamples+1);
                        bool lsb = ExtractBitFromByte(mappedVal.DataFileByte, byteMapping.BitPosition);
                        samplesOut[i] = ByteHelper.ModifyIntLSB(samplesIn[i], lsb);
                    } else
                    {
                        samplesOut[i] = samplesIn[i];
                    }
                }
                WriteCallback(samplesOut);
                TotalReadSamples += (uint)samplesIn.Length;
            }

            public void ProcessAudiofile(Action<int[]> writeCallback)
            {
                WriteCallback = writeCallback ?? throw new ArgumentNullException("writeCallback");
                ReadInDataFile();
                AudioFileStream.ReadFile(ProcessSampleBlock);
            }

            private void GenerateMapping(byte dataByteIn)
            {
                DataByteMapping ret = new DataByteMapping(dataByteIn);
                for (int i = 0; i < 8; i++)
                {
                    uint generated = Rng.Next(AudioTotalSamples);
                    while (PositionMapping.ContainsKey(generated))
                    {
                        generated = Rng.Next(AudioTotalSamples);
                    };
                    ret.MappedBytes[i] = generated;
                    PositionMapping[generated] = ret;
                }
            }

            private void ReadInDataFile()
            {
                int readByte = DataFileStream.ReadByte();
                while (readByte != -1)
                {
                    byte dataByte = (byte)readByte;
                    GenerateMapping(dataByte);
                    readByte = DataFileStream.ReadByte();
                }
            }
        }
    }
}
