using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography.Audio.Flac
{
    public class FlacVerbatimSubframe : FlacSubFrame
    {

        private int[] ContainedSamples;

        public FlacVerbatimSubframe(FlacAudioFrame parentFrame) : base(parentFrame) { }



        public void ReadContainedSamples(BitReader readerIn)
        {
            ContainedSamples = new int[ParentFrame.InterChannelSampleBlockSize];
            int bitsToRead = ParentFrame.SampleSize - Header.WastedBitsPerSample;
            for (int i = 0; i < ContainedSamples.Length; i++)
                ContainedSamples[i] = (int)readerIn.ReadSpecifiedBitCount(bitsToRead);
        }
    }
}
