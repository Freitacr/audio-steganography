using System.Collections.Generic;
using System.Linq;
using System.IO;
using FlaCdotNet.Metadata;
using AudioSteganography.Audio;
using AudioSteganography.Helper;
using System;

namespace AudioSteganography
{
    namespace Steganography
    {
        class DataFileByteBitAssociation
        {
            public int DataFileBitPosition;
            public bool BitValue;
            public DataFileByteBitAssociation(int bitPosition, bool bitValue)
            {
                DataFileBitPosition = bitPosition;
                BitValue = bitValue;
            }
        }

        public class NLSBSteganographerDecoder
        {
            private FileInDecodingStream AudioFileStream;
            private Stream DataFileStreamOut;
            private KeyedRng Rng;
            private int DataFileSize = -1;
            private Dictionary<uint, DataByteMapping> PositionMapping = new Dictionary<uint, DataByteMapping>();
            private List<DataByteMapping> DataFileBytes = new List<DataByteMapping>();
            private uint AudioTotalSamples;
            private uint TotalSamplesRead = 0;

            public NLSBSteganographerDecoder(FileInDecodingStream audioFileStream, Stream dataFileStreamOut, byte[] key)
            {
                AudioFileStream = audioFileStream;
                DataFileStreamOut = dataFileStreamOut;
                Rng = new KeyedRng(key);
                AudioFileStream.MetadataCallback = MetadataCallback;
            }

            private void MetadataCallback(Prototype metadataIn)
            {
                if (DataFileSize != -1 && metadataIn.MetadataType == FlaCdotNet.MetadataType.VorbisComment)
                    return;
                if (metadataIn.MetadataType == FlaCdotNet.MetadataType.VorbisComment)
                {
                    var metadata = metadataIn as VorbisComment;

                    for (uint i = 0; i < metadata.NumComments; i++)
                    {
                        if (metadata.GetComment(i).FieldName == "LEN")
                        {
                            DataFileSize = int.Parse(metadata.GetComment(i).FieldValue);
                            for (int j = 0; j < DataFileSize; j++)
                                GenerateMapping();
                            return;
                        }
                    }
                } else if (metadataIn.MetadataType == FlaCdotNet.MetadataType.StreamInfo)
                {
                    var metadata = metadataIn as StreamInfo;
                    AudioTotalSamples = (uint)(metadata.GetChannels() * metadata.GetTotalSamples());
                }
            }

            private void GenerateMapping()
            {
                DataByteMapping ret = new DataByteMapping(0);
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
                DataFileBytes.Add(ret);
            }

            private byte UpdateByteWithBit(byte byteIn, bool bitIn, int bitPosition)
            {
                int bitMask = bitIn ? 0x1 : 0x0;
                bitMask = bitMask << bitPosition;
                return (byte)(byteIn | bitMask);
            }

            private bool ExtractLSB(int byteIn)
            {
                return (byteIn & 0x1) != 0;
            }

            private void AudioWriteCallback(int[] samplesIn)
            {
                for (int i = 0; i < samplesIn.Length; i++)
                {
                    if (PositionMapping.TryGetValue((uint)(i+TotalSamplesRead), out DataByteMapping mappedVal))
                    {
                        var byteMapping = mappedVal.TryGetByteMapping((uint)(i + TotalSamplesRead), AudioTotalSamples + 1);
                        bool lsb = ExtractLSB(samplesIn[i]);
                        mappedVal.DataFileByte = UpdateByteWithBit(mappedVal.DataFileByte, lsb, byteMapping.BitPosition);
                    }
                }
                TotalSamplesRead += (uint)samplesIn.Length;
            }

            public void ProcessAudioFile()
            {
                AudioFileStream.ReadFile(AudioWriteCallback);
                foreach (DataByteMapping b in DataFileBytes)
                    DataFileStreamOut.WriteByte(b.DataFileByte);
            }
        }
    }
}
