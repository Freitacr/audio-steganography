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
            private int BitPermutationIndicesRemaining { get { return DataFileSize - IndicesUsed; } }
            private List<int> BitPermutationIndices = new List<int>();
            private int IndicesUsed = 0;
            private LinkedList<DataFileByteBitAssociation> ByteBitAssociations = new LinkedList<DataFileByteBitAssociation>();
            private Dictionary<int, DataFileByteBitAssociation> HashedAssociations = new Dictionary<int, DataFileByteBitAssociation>();
            private KeyedRng Rng;
            private int HighestConsecutivePositionProcessed = -1;
            private int DataFileSize = -1;

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
                            DataFileSize = int.Parse(metadata.GetComment(i).FieldValue) * 8;
                            for (int j = 0; j < DataFileSize; j++)
                                BitPermutationIndices.Add(j);
                            return;
                        }
                    }
                } else if (metadataIn.MetadataType == FlaCdotNet.MetadataType.StreamInfo)
                {
                    var metadata = metadataIn as StreamInfo;
                    StreamInfoPrinter.PrintStreamInfoConsole(metadata);
                }
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

            private void CheckAssociationsForConsecutivity()
            {
                bool checkNextByte = true;
                while (checkNextByte)
                {
                    checkNextByte = false;
                    int checkFor = HighestConsecutivePositionProcessed + 1;
                    var validAssociations = new List<DataFileByteBitAssociation>();
                    for (int i = 0; i < 8; i++)
                    {
                        int bitPos = checkFor + i;
                        if (HashedAssociations.TryGetValue(bitPos, out var curr))
                        {
                            validAssociations.Add(curr);
                        }
                        else break;
                    }
                    if (validAssociations.Count == 8)
                    {
                        byte toWrite = 0x0;
                        checkNextByte = true;
                        foreach (var assoc in validAssociations)
                        {
                            toWrite = UpdateByteWithBit(toWrite, assoc.BitValue, assoc.DataFileBitPosition % 8);
                            HashedAssociations.Remove(assoc.DataFileBitPosition);
                        }
                        DataFileStreamOut.WriteByte(toWrite);
                        HighestConsecutivePositionProcessed += 8;
                    }
                }
            }

            private void AudioFileWriteCallback(int[] samplesIn)
            {
                if (BitPermutationIndicesRemaining == 0)
                {
                    BitPermutationIndices.Clear();
                    return;
                }
                foreach (int sample in samplesIn)
                {
                    if (BitPermutationIndices.Count - IndicesUsed == 0)
                        continue;
                    int generatedIndex = Rng.Next(BitPermutationIndicesRemaining);
                    int bitPosition = BitPermutationIndices[generatedIndex];
                    IndicesUsed++;
                    int temp = BitPermutationIndices[generatedIndex];
                    BitPermutationIndices[generatedIndex] = BitPermutationIndices[BitPermutationIndicesRemaining];
                    BitPermutationIndices[BitPermutationIndicesRemaining] = temp;
                    HashedAssociations[bitPosition] = new DataFileByteBitAssociation(bitPosition, ExtractLSB(sample));
                }
                CheckAssociationsForConsecutivity();
            }

            public List<int> GetPermutationIndicies(int numGenerate)
            {
                BitPermutationIndices = new List<int>();
                for (int i = 0; i < numGenerate; i++)
                    BitPermutationIndices.Add(i);
                List<int> ret = new List<int>();
                for (int i = 0; i < numGenerate; i++)
                {
                    int remaining = numGenerate - i;
                    int generatedIndex = Rng.Next(remaining);
                    ret.Add(BitPermutationIndices[generatedIndex]);
                    int temp = BitPermutationIndices[generatedIndex];
                    BitPermutationIndices[generatedIndex] = BitPermutationIndices[remaining-1];
                    BitPermutationIndices[remaining-1] = temp;
                }
                return ret;
            }

            public void ProcessFile()
            {
                Console.WriteLine("Decoder's Decoder info:");
                AudioFileStream.ReadFile(AudioFileWriteCallback);
            }

            
        }
    }
}
