using System;
using System.Collections.Generic;
using AudioSteganography.Helper;
using System.Text;

namespace AudioSteganography.Audio.Flac
{

    public class FlacResidual
    {
        private RicePartitionMethod ContainedPartitions;
        public FlacSubFrame ParentFrame { get; private set; }
        public FlacResidual(FlacSubFrame parentFrame)
        {
            ParentFrame = parentFrame;
        }

        public void ParseResidual(BitReader readerIn)
        {
            if (readerIn.ReadBit())
                throw new ImproperFormatException("Residual Coding Method was a reserved value...");
            bool rice2 = readerIn.ReadBit();
            ContainedPartitions = new RicePartitionMethod(this, rice2);
            ContainedPartitions.ParsePartition(readerIn);
        }
    }

    public class RicePartitionMethod
    {
        public int PartitionOrder { get; protected set;}
        public FlacResidual ParentResidual { get; private set; }
        private List<RicePartition> ContainedPartitions;
        private bool IsRice2;

        public RicePartitionMethod(FlacResidual parentResidual, bool isRice2)
        {
            ParentResidual = parentResidual;
            ContainedPartitions = new List<RicePartition>();
            IsRice2 = isRice2;
        }

        public void ParsePartition(BitReader readerIn) {
            ParsePartitionOrder(readerIn);
            for (int i = 0; i < (int)Math.Pow(2, PartitionOrder); i++)
            {
                RicePartition currRicePartition = IsRice2 ? (RicePartition)new Rice2Partition(this) : new Rice1Partition(this);
                ContainedPartitions.Add(currRicePartition);
                currRicePartition.ReadPartition(readerIn, i == 0);
            }
        }

        protected void ParsePartitionOrder(BitReader readerIn)
        {
            PartitionOrder = (int)readerIn.ReadSpecifiedBitCount(4);
        }
    }

    public abstract class RicePartition
    {
        protected byte RiceParameter;
        protected int[] ContainedSamples;
        protected bool UnencodedBinary = false;
        public RicePartitionMethod ParentPartitionMethod { get; private set; }

        public RicePartition(RicePartitionMethod parentPartitionMethod)
        {
            ParentPartitionMethod = parentPartitionMethod;
        }
        public void ReadPartition(BitReader readerIn, bool isFirst)
        {
            ReadRiceParameter(readerIn);
            uint numSamples;
            uint blockSize = (uint)ParentPartitionMethod.ParentResidual.ParentFrame.ParentFrame.InterChannelSampleBlockSize;
            uint predictorOrder = ParentPartitionMethod.ParentResidual.ParentFrame.Header.Order;
            uint partitionOrder = (uint)ParentPartitionMethod.PartitionOrder;
            if (ParentPartitionMethod.PartitionOrder == 0)
            {
                numSamples = blockSize - predictorOrder;
            }
            else if (!isFirst)
            {
                numSamples = blockSize / (uint)Math.Pow(2, partitionOrder);
            }
            else
            {
                numSamples = blockSize / (uint)Math.Pow(2, partitionOrder);
                numSamples -= predictorOrder;
            }
            ContainedSamples = new int[numSamples];
            if (UnencodedBinary)
            {
                /**
                 * RiceParameter contains the number of bits to read in per sample
                 */
                for (int i = 0; i < ContainedSamples.Length; i++)
                    ContainedSamples[i] = (int)readerIn.ReadSpecifiedBitCount(RiceParameter);
            }
            else
            {
                for (int i = 0; i < ContainedSamples.Length; i++)
                    ContainedSamples[i] = RiceHelper.ReadFlacRiceEncodedInt(RiceParameter, readerIn);
            }
        }

        public abstract void ReadRiceParameter(BitReader readerIn);
    }

    public class Rice1Partition : RicePartition
    {
        public Rice1Partition (RicePartitionMethod parentPartitionMethod) : base(parentPartitionMethod)
        {

        }

        public override void ReadRiceParameter(BitReader readerIn)
        {
            RiceParameter = (byte)readerIn.ReadSpecifiedBitCount(4);
            if (RiceParameter == 0xF)
            {
                RiceParameter = (byte)readerIn.ReadSpecifiedBitCount(5);
                UnencodedBinary = true;
            }
        }
    }

    public class Rice2Partition : RicePartition
    {
        public Rice2Partition(RicePartitionMethod parentPartitionMethod) : base(parentPartitionMethod)
        {

        }

        public override void ReadRiceParameter(BitReader readerIn)
        {
            RiceParameter = (byte)readerIn.ReadSpecifiedBitCount(5);
            if (RiceParameter == 0x1F)
            {
                RiceParameter = (byte)readerIn.ReadSpecifiedBitCount(5);
                UnencodedBinary = true;
            }
        }
    }
}
