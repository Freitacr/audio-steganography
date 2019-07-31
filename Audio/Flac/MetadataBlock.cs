using System.IO;
using AudioSteganography.Helper;

namespace AudioSteganography.Audio.Flac
{
    public class MetadataBlock
    {
        public enum BLOCK_TYPE
        {
            STREAMINFO,
            PADDING,
            APPLICATION,
            SEEKTABLE,
            VORBIS_COMMENT,
            CUESHEET,
            PICTURE,
            UNKNOWN
        }

        public int Length { get; protected set; } = 0;
        public bool IsLast { get; protected set; } = false;
        public BLOCK_TYPE MetadataType { get; protected set; } = BLOCK_TYPE.UNKNOWN;
        public byte[] Data { get; protected set; }

        public static MetadataBlock ReadMetadataBlock(Stream readerIn)
        {
            MetadataBlock readBlock = new MetadataBlock();
            int header = ByteHelper.ReadInt32(readerIn);
            readBlock.IsLast = MetadataBlockHelper.ExtractIsLast(header);
            readBlock.MetadataType = MetadataBlockHelper.ExtractBlockType(header);
            if (readBlock.MetadataType == BLOCK_TYPE.PADDING && readBlock.Length == 0)
            {
                byte readByte = 0;
                while (readByte == 0)
                    readByte = (byte)readerIn.ReadByte();
                readerIn.Seek(-1, SeekOrigin.Current);
            }
            else
            {
                readBlock.Length = MetadataBlockHelper.ExtractLength(header);
                readBlock.Data = new byte[readBlock.Length];
                for (int i = 0; i < readBlock.Data.Length; i++)
                    readBlock.Data[i] = (byte)readerIn.ReadByte();
            }
            return SubclassMetadataBlock(readBlock);
        }

        private static MetadataBlock SubclassMetadataBlock(MetadataBlock blockIn)
        {
            switch(blockIn.MetadataType)
            {
                case BLOCK_TYPE.STREAMINFO:
                    return new StreamInfo(blockIn);
                case BLOCK_TYPE.VORBIS_COMMENT:
                    return new VorbisComment(blockIn);
                default:
                    return blockIn;
            }
        }
    }

    internal static class MetadataBlockHelper
    {
        public static bool ExtractIsLast(int headerIn)
        {
            int mask = 0x8000000 << 4;
            int res = mask & headerIn;
            return res != 0;
        }

        public static MetadataBlock.BLOCK_TYPE ExtractBlockType(int headerIn)
        {
            int mask = 0x7f000000;
            int res = (mask & headerIn) >> 24;
            switch (res)
            {
                case 0:
                    return MetadataBlock.BLOCK_TYPE.STREAMINFO;
                case 1:
                    return MetadataBlock.BLOCK_TYPE.PADDING;
                case 2:
                    return MetadataBlock.BLOCK_TYPE.APPLICATION;
                case 3:
                    return MetadataBlock.BLOCK_TYPE.SEEKTABLE;
                case 4:
                    return MetadataBlock.BLOCK_TYPE.VORBIS_COMMENT;
                case 5:
                    return MetadataBlock.BLOCK_TYPE.CUESHEET;
                case 6:
                    return MetadataBlock.BLOCK_TYPE.PICTURE;
                default:
                    return MetadataBlock.BLOCK_TYPE.UNKNOWN;
            }
        }

        public static int ExtractLength (int headerIn)
        {
            int mask = 0x00fff;
            int res = mask & headerIn;
            return res;
        }
    }
}
