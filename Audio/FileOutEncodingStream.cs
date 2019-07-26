using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using enc = FlaCdotNet.Encoder;
using FlaCdotNet.Metadata;

namespace AudioSteganography
{
    namespace Audio
    {
        public class FileOutEncodingStream : enc.Stream
        {
            private StreamWriter FileOut;
            private uint BytesWritten = 0;
            public Action<double> ProgressCallback = null;
            public StreamWriter InfoFileWriter;

            public FileOutEncodingStream(string filePath)
            {
                FileOut = new StreamWriter(filePath);
            }

            public enc.InitStatus InitializeStream(uint channels, uint bitsPerSample, uint sampleRate, uint samplesEstimate, bool verify = true)
            {
                if (!SetChannels(channels))
                    return enc.InitStatus.InvalidNumberOfChannels;
                if (!SetBitsPerSample(bitsPerSample))
                    return enc.InitStatus.InvalidBitsPerSample;
                if (!SetSampleRate(sampleRate))
                    return enc.InitStatus.InvalidSampleRate;
                if (!SetTotalSamplesEstimate(samplesEstimate))
                    return enc.InitStatus.InvalidCallbacks;

                SetVerify(verify);
                return Init();
            }

            protected override void metadataCallback(Prototype metadataIn)
            {
                Console.WriteLine("metaCallback Called");
            }

            protected override enc.WriteStatus writeCallback(byte[] buffer, uint bytes, uint samples, uint currentFrame)
            {
                try
                {
                    FileOut.BaseStream.Write(buffer, 0, (int)bytes);
                }
                catch (Exception)
                {
                    return enc.WriteStatus.FatalError;
                }
                BytesWritten += bytes;
                ProgressCallback?.Invoke(BytesWritten / (double)GetTotalSamplesEstimate());
                if (InfoFileWriter != null)
                    InfoFileWriter.WriteLine(currentFrame);
                return enc.WriteStatus.Ok;
            }

            public void Close()
            {
                Finish();
                FileOut.Close();
                Dispose();
            }
        }
    }
}
