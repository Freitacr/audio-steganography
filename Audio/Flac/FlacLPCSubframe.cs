using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography.Audio.Flac
{
    

    public class FlacLPCSubframe : FlacSubFrame
    {
        private int[] WarmupSamples;
        private byte CoefficientPrecision;
        private short CoefficientShift;
        private int[] PredictorCoefficients;
        private FlacResidual ContainedResidual;

        public FlacLPCSubframe(FlacAudioFrame parentFrame) : base(parentFrame)
        {

        }

        public void ReadContainedSamples(BitReader readerIn)
        {
            byte[] streamOracle = new byte[32];
            for (int i = 0; i < streamOracle.Length; i++)
            {
                streamOracle[i] = readerIn.ReadByte();
            }
            throw new NotImplementedException();
            ReadWarmupSamples(readerIn);
            ReadCoefficientPrecision(readerIn);
            ReadCoefficientShift(readerIn);
            ReadPredictorCoefficients(readerIn);
            ContainedResidual = new FlacResidual(this);
            ContainedResidual.ParseResidual(readerIn);
        }

        private void ReadWarmupSamples(BitReader readerIn)
        {
            int bitsToRead = ParentFrame.SampleSize - Header.WastedBitsPerSample;
            WarmupSamples = new int[Header.Order];
            for (int i = 0; i < Header.Order; i++)
                WarmupSamples[i] = (int)readerIn.ReadSpecifiedBitCount(bitsToRead);
        }

        private void ReadCoefficientPrecision(BitReader readerIn)
        {
            CoefficientPrecision = (byte)(readerIn.ReadSpecifiedBitCount(4)+1);
            if ((CoefficientPrecision ^ 0x10) == 0)
                throw new ImproperFormatException("LPC Subframe Coefficient Precision was the invalid value");
        }

        private void ReadCoefficientShift(BitReader readerIn)
        {
            CoefficientShift = (short)(readerIn.ReadSpecifiedBitCount(5));
            CoefficientShift -= (short)(0x10 & CoefficientShift);
        }

        private void ReadPredictorCoefficients(BitReader readerIn)
        {
            PredictorCoefficients = new int[Header.Order];
            for (int i = 0; i < PredictorCoefficients.Length; i++)
                PredictorCoefficients[i] = (int)readerIn.ReadSpecifiedBitCount(CoefficientPrecision);
        }
    }
}
