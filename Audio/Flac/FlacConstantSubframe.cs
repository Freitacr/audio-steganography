using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography.Audio.Flac
{
    public class FlacConstantSubframe : FlacSubFrame
    {
        public FlacConstantSubframe(FlacAudioFrame parentFrame) : base(parentFrame) { }
        private int[] ContainedSamples;


        public void ReadContainedSamples(BitReader readerIn)
        {
            int bitsToRead = ParentFrame.SampleSize - Header.WastedBitsPerSample;
            int constantSample = (int)readerIn.ReadSpecifiedBitCount(bitsToRead);
            ContainedSamples = new int[ParentFrame.InterChannelSampleBlockSize];
            for (int i = 0; i < ContainedSamples.Length; i++)
            {
                ContainedSamples[i] = constantSample;
            }
        }
    }
}
