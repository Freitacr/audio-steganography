using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using dec = FlaCdotNet.Decoder;
using FlaCdotNet.Metadata;
using FlaCdotNet;

namespace AudioSteganography
{
    namespace Audio
    {
        public class NotInitializedException : Exception
        {
            public NotInitializedException(string message) : base(message) { }
        }

        public class FileInDecodingStream : dec.File
        {

            private List<List<int>> FileContents;
            private uint Channels = 0;
            private uint BitsPerSample = 0;
            private uint SampleRate = 0;
            private uint SamplesPerChannel = 0;
            private bool Initialized = false;
            private Action<int[]> SuccessfulWriteCallback = null;
            public Action<Prototype> MetadataCallback = null;
            public Action<double> ProgressCallback = null;
            public StreamWriter InfoOutFile;

            public int[] GetDataInterleaved()
            {
                if (FileContents == null)
                    throw new NotInitializedException("File Decoding Stream has not been initialized yet.");
                int[] ret = new int[FileContents.Count * FileContents[0].Count];
                int currIndex = 0;
                for (int i = 0; i < FileContents[0].Count; i++)
                {
                    foreach (List<int> currList in FileContents)
                    {
                        ret[currIndex] = currList[i];
                        currIndex++;
                    }
                }
                return ret;
            }

            public int[,] GetDataChannelSplit()
            {
                if (FileContents == null)
                    throw new NotInitializedException("File Decoding Stream has not been initialized yet.");
                int[,] ret = new int[FileContents.Count, FileContents[0].Count];
                for (int i = 0; i < FileContents.Count; i++)
                    for (int j = 0; j < FileContents[0].Count; j++)
                        ret[i, j] = FileContents[i][j];
                return ret;
            }

            public void ReadFile()
            {
                if (!Initialized)
                    throw new NotInitializedException("The current stream has not been initialized yet.");
                ProcessUntilEndOfStream();
            }

            public void Initialize(string fileName)
            {
                Init(fileName);
                ProcessUntilEndOfMetadata();
                Initialized = true;
            }

            public bool IsInitialized()
            {
                return Initialized;
            }

            protected override void errorCallback(dec.ErrorStatus status)
            {
                throw new NotImplementedException();
            }

            protected override void metadataCallback(Prototype metadataIn)
            {
                MetadataCallback?.Invoke(metadataIn);
            }

            protected override dec.WriteStatus writeCallback(Frame frame, int[,] buffer)
            {
                if (FileContents == null)
                {
                    FileContents = new List<List<int>>();
                    for (int i = 0; i < frame.Header.Channels; i++)
                        FileContents.Add(new List<int>());
                    Channels = frame.Header.Channels;
                    BitsPerSample = frame.Header.BitsPerSample;
                    SampleRate = frame.Header.SampleRate;
                }
                for (int i = 0; i < frame.Header.Channels; i++)
                {
                    for (int j = 0; j < frame.Header.BlockSize; j++)
                    {
                        FileContents[i].Add(buffer[i, j]);
                    }
                }
                SamplesPerChannel += frame.Header.BlockSize;
                if (SuccessfulWriteCallback != null)
                {
                    SuccessfulWriteCallback(GetDataInterleaved());
                    ClearReadData();
                }
                ProgressCallback?.Invoke(SamplesPerChannel / (double)GetTotalSamples());
                if (InfoOutFile != null)
                {
                    InfoOutFile.WriteLine(frame.Header.FrameNumber);
                }
                return dec.WriteStatus.Continue;
            }

            public void ClearReadData()
            {
                foreach (List<int> ls in FileContents)
                    ls.Clear();
            }

            public override uint GetChannels()
            {
                return Channels;
            }

            public override uint GetBitsPerSample()
            {
                return BitsPerSample;
            }

            public override uint GetSampleRate()
            {
                return SampleRate;
            }

            public void Close()
            {
                Finish();
                Dispose();
            }

            public void WriteErrorState()
            {
                dec.State state = GetState();
                Console.WriteLine(state.ToString());
            }

            public void ReadFile(Action<int[]> writeCallback)
            {
                SuccessfulWriteCallback = writeCallback;
                ReadFile();
            }
        }
    }
}
