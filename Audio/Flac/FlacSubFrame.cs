using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AudioSteganography.Audio.Flac
{
    public enum SUBFRAME_TYPE
    {
        CONSTANT,
        FIXED,
        LPC,
        VERBATIM
    }

    public class FlacSubFrame
    {

        public FlacSubFrameHeader Header { get; private set; }
        public FlacAudioFrame ParentFrame { get; protected set; }

        public FlacSubFrame(FlacAudioFrame parentFrame)
        {
            ParentFrame = parentFrame;
        }

        public static FlacSubFrame ReadSubFrame(BitReader streamIn, FlacAudioFrame parentFrame)
        {
            FlacSubFrame ret = new FlacSubFrame(parentFrame);
            ret.Header = new FlacSubFrameHeader(streamIn);
            return SubclassSubFrame(ret, streamIn);
        }

        private static FlacSubFrame SubclassSubFrame(FlacSubFrame baseFrame, BitReader streamIn)
        {
            FlacSubFrame ret;
            switch(baseFrame.Header.SubframeType)
            {
                case SUBFRAME_TYPE.FIXED:
                    ret = new FlacFixedSubframe(baseFrame.ParentFrame);
                    ret.Header = baseFrame.Header;
                    (ret as FlacFixedSubframe).ReadContainedSamples(streamIn);
                    return ret;
                case SUBFRAME_TYPE.CONSTANT:
                    ret = new FlacConstantSubframe(baseFrame.ParentFrame);
                    ret.Header = baseFrame.Header;
                    (ret as FlacConstantSubframe).ReadContainedSamples(streamIn);
                    return ret;
                case SUBFRAME_TYPE.LPC:
                    ret = new FlacLPCSubframe(baseFrame.ParentFrame);
                    ret.Header = baseFrame.Header;
                    (ret as FlacLPCSubframe).ReadContainedSamples(streamIn);
                    return ret;
                case SUBFRAME_TYPE.VERBATIM:
                    ret = new FlacVerbatimSubframe(baseFrame.ParentFrame);
                    ret.Header = baseFrame.Header;
                    (ret as FlacVerbatimSubframe).ReadContainedSamples(streamIn);
                    return ret;
                default:
                    throw new ImproperFormatException("Impossible subframe type encountered....");
            }
        }
    }

    public class FlacSubFrameHeader
    {
        public SUBFRAME_TYPE SubframeType { get; private set; }
        public int WastedBitsPerSample { get; private set; }
        public uint Order { get; private set; }
        public FlacSubFrameHeader(BitReader streamIn)
        {
            ReadSelf(streamIn);
            
        }

        private void ReadSelf(BitReader streamIn)
        {
            if (streamIn.ReadBit())
                throw new ImproperFormatException("Expected first bit of header to be a 0. Received a 1 instead");
            uint subframeType = streamIn.ReadSpecifiedBitCount(6);
            SetSubframeTypeFromValue(subframeType);
            if (!streamIn.ReadBit())
            {
                WastedBitsPerSample = 0;
                return;
            }
            WastedBitsPerSample = (int)streamIn.ReadUnary();
        } 

        private void SetSubframeTypeFromValue(uint value)
        {
            switch(value)
            {
                case 0:
                    SubframeType = SUBFRAME_TYPE.CONSTANT;
                    break;
                case 1:
                    SubframeType = SUBFRAME_TYPE.VERBATIM;
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    throw new ImproperFormatException("Reserved Subframe Type found...");
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                    SubframeType = SUBFRAME_TYPE.FIXED;
                    uint orderMask = ~0x8u;
                    Order = value & orderMask;
                    break;
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                    throw new ImproperFormatException("Reserved Subframe Type found...");
                default:
                    SubframeType = SUBFRAME_TYPE.LPC;
                    uint mask = ~0x20u;
                    Order = value & mask;
                    Order++;
                    break;
            }
        }
    }
}
