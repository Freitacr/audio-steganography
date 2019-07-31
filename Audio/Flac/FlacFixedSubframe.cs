using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography.Audio.Flac
{
    public class FlacFixedSubframe : FlacSubFrame
    {
        private int[] WarmupSamples;
        private FlacResidual ContainedResidual;

        public FlacFixedSubframe(FlacAudioFrame parentFrame) : base(parentFrame) {
            
        }

        public void ReadContainedSamples(BitReader readerIn)
        {
            ReadWarmupSamples(readerIn);
            ContainedResidual = new FlacResidual(this);
            ContainedResidual.ParseResidual(readerIn);
        }

        private void ReadWarmupSamples(BitReader readerIn)
        {
            uint numSamplesToRead = Header.Order;
            WarmupSamples = new int[numSamplesToRead];
            int bitsToRead = ParentFrame.SampleSize - Header.WastedBitsPerSample;
            for (uint i = 0; i < numSamplesToRead; i++)
                WarmupSamples[i] = (int)readerIn.ReadSpecifiedBitCount(bitsToRead);
        }
    }
}
