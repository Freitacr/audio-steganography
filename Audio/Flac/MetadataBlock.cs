using System.IO;

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

        public int Length { get; private set; } = 0;
        public bool IsLast { get; private set; } = false;
        public BLOCK_TYPE MetadataType { get; private set; } = BLOCK_TYPE.UNKNOWN;
        public byte[] Data { get; private set; }

        public void ReadMetadataBlock(BinaryReader readerIn)
        {
            int header = readerIn.ReadInt32();
            IsLast = MetadataBlockHelper.ExtractIsLast(header);
            MetadataType = MetadataBlockHelper.ExtractBlockType(header);
            Length = MetadataBlockHelper.ExtractLength(header);
            Data = new byte[Length];
            for (int i = 0; i < Data.Length; i++)
                Data[i] = readerIn.ReadByte();
        }
    }

    internal static class MetadataBlockHelper
    {
        public static bool ExtractIsLast(int headerIn)
        {
            int mask = 0x8000;
            int res = mask & headerIn;
            return res != 0;
        }

        public static MetadataBlock.BLOCK_TYPE ExtractBlockType(int headerIn)
        {
            int mask = 0x7f000;
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
