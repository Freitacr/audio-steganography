using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlaCdotNet.Metadata;

namespace AudioSteganography.Helper
{
    public class StreamInfoPrinter
    {
        public static void PrintStreamInfoConsole(StreamInfo infoIn)
        {
            Console.WriteLine("Max Block Size: " + infoIn.MaxBlockSize);
            Console.WriteLine("Min Block Size: " + infoIn.MinBlockSize);
            Console.WriteLine("Max Frame Size: " + infoIn.MaxFrameSize);
            Console.WriteLine("Min Frame Size: " + infoIn.MinFrameSize);
            Console.WriteLine("Sample Rate: " + infoIn.SampleRate);
            Console.WriteLine("Bits Per Sample: " + infoIn.BitsPerSample);
        }
    }
}
