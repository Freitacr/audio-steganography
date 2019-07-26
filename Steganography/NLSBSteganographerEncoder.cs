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

            public void ModifyByte(bool lsbValue)
            {
                int lsb = lsbValue ? 0x1 : 0x0;
                int mask = 0xfe;
                int res = AudioByte & mask;
                AudioByte = res | lsb;
                Modified = true;
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
            private List<int> BitPermutationIndices = new List<int>();
            private int IndicesUsed = 0;
            private int BitPermutationIndicesRemaining { get { return BitPermutationIndices.Count - IndicesUsed; } }
            private LinkedList<AudioByteDataBitAssociation> AudioByteAssociations = new LinkedList<AudioByteDataBitAssociation>();
            private Dictionary<int, AudioByteDataBitAssociation> HashedAssociations = new Dictionary<int, AudioByteDataBitAssociation>();
            private ulong ProcessedSamples = 0;
            private KeyedRng Rng;
            private int HighestConsecutivePositionProcessed = -1;
            public Action<int[]> WriteCallback = null;

            /**
             * <summary>audioFileStream must be initialized prior to passing
             * dataFileStream must be open prior to passing</summary>
             */
            public NLSBSteganographerEncoder(FileInDecodingStream audioFileStream, Stream dataFileStream, byte[] key, int dataFileSize)
            {
                if ((audioFileStream.GetTotalSamples()*audioFileStream.GetChannels()) < ((uint)dataFileSize * 8))
                    throw new InsufficientAudioException();
                AudioFileStream = audioFileStream;
                AudioFileStream.MetadataCallback = MetadataCallback;
                DataFileStream = dataFileStream;
                for (int i = 0; i < dataFileSize * 8; i++)
                    BitPermutationIndices.Add(i);
                Rng = new KeyedRng(key);
            }

            public static void MetadataCallback(Prototype metadataIn)
            {
                if (metadataIn.MetadataType == FlaCdotNet.MetadataType.StreamInfo)
                {
                    var metadata = metadataIn as StreamInfo;
                    StreamInfoPrinter.PrintStreamInfoConsole(metadata);
                }
            }

            public void Clear()
            {
                BitPermutationIndices.Clear();
                AudioByteAssociations.Clear();
                HashedAssociations.Clear();
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

            

            private void ReadInNecessaryData()
            {
                if (AudioByteAssociations.Count == 0)
                    return;
                bool checkNextByte = true;
                while (checkNextByte)
                {
                    List<AudioByteDataBitAssociation> validAssociations = new List<AudioByteDataBitAssociation>();
                    checkNextByte = false;
                    int bitPosition = (HighestConsecutivePositionProcessed+1);
                    for (int i = 0; i < 8; i++)
                    {
                        int bitPos = bitPosition + i;
                        if (HashedAssociations.TryGetValue(bitPos, out var curr))
                            validAssociations.Add(curr);
                        else break;
                    }
                    //Yeah, this means we wait to read in a byte until we need the whole thing.
                    if (validAssociations.Count == 8)
                    {
                        int readByte = DataFileStream.ReadByte();
                        foreach (AudioByteDataBitAssociation assoc in validAssociations)
                            assoc.ModifyByte(ExtractBitFromByte(readByte, assoc.BitPosition % 8));
                        HighestConsecutivePositionProcessed += 8;
                        checkNextByte = true;
                        for (int i = 0; i < 8; i++)
                        {
                            int bitPos = bitPosition + i;
                            HashedAssociations.Remove(i);
                        }
                    }
                }
            }

            private void WriteModifiedAssociations(Action<int[]> writeCallback)
            {
                List<AudioByteDataBitAssociation> modifiedSamples = new List<AudioByteDataBitAssociation>();
                foreach (AudioByteDataBitAssociation assoc in AudioByteAssociations)
                {
                    if (assoc.Modified)
                        modifiedSamples.Add(assoc);
                    else break;
                }
                foreach (var assoc in modifiedSamples)
                    AudioByteAssociations.RemoveFirst();
                List<int> samples = new List<int>();
                foreach (var assoc in modifiedSamples)
                    samples.Add(assoc.AudioByte);
                if (samples.Count > 0)
                    writeCallback(samples.ToArray());
            }

            public void ProcessBlockFromFile(int[] samplesIn)
            {
                if (WriteCallback == null)
                    throw new ArgumentNullException("WriteCallback", "Public Field WriteCallback must be set prior to using this method");
                if (BitPermutationIndicesRemaining == 0)
                    BitPermutationIndices.Clear();
                foreach (int sample in samplesIn)
                {
                    if (BitPermutationIndicesRemaining <= 0)
                    {
                        AudioByteDataBitAssociation toAdd = new AudioByteDataBitAssociation(sample, 0);
                        toAdd.Modified = true;
                        AudioByteAssociations.AddLast(toAdd);
                    }
                    else
                    {
                        int generatedIndex = Rng.Next(BitPermutationIndicesRemaining);
                        var assoc = new AudioByteDataBitAssociation(sample, BitPermutationIndices[generatedIndex]);
                        AudioByteAssociations.AddLast(assoc);
                        HashedAssociations[assoc.BitPosition] = assoc;
                        IndicesUsed++;
                        int temp = BitPermutationIndices[generatedIndex];
                        BitPermutationIndices[generatedIndex] = BitPermutationIndices[BitPermutationIndicesRemaining];
                        BitPermutationIndices[BitPermutationIndicesRemaining] = temp;
                    }
                    ProcessedSamples++;
                }
                ReadInNecessaryData();
                WriteModifiedAssociations(WriteCallback);
            }

            public void ProcessFile(Action<int[]> writeCallback)
            {
                WriteCallback = writeCallback;
                Console.WriteLine("Encoder's Decoder information: ");
                AudioFileStream.ReadFile(ProcessBlockFromFile);
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
        }
    }
}
