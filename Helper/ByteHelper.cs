using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AudioSteganography.Helper
{
    public class ByteHelper
    {
        public static byte ReverseEndian(byte byteIn)
        {
            
            uint mask = 0x80;
            uint ret = 0x0;
            ret |= ((mask >> 7) & byteIn) << 7;
            ret |= ((mask >> 6) & byteIn) << 5;
            ret |= ((mask >> 5) & byteIn) << 3;
            ret |= ((mask >> 4) & byteIn) << 1;
            ret |= ((mask >> 3) & byteIn) >> 1;
            ret |= ((mask >> 2) & byteIn) >> 3;
            ret |= ((mask >> 1) & byteIn) >> 5;
            ret |= (mask & byteIn) >> 7;
            return (byte)ret;
        }

        public static int ReverseEndian(int intIn)
        {
            uint mask = 0x80000000;
            uint ret = 0x0;
            uint iin = (uint)intIn;
            int maskCounter = 31;
            for (int i = maskCounter; i >=1; i-=2, maskCounter--)
                ret |= ((mask >> maskCounter) & iin) << i;
            for (int i = 1; i < 31; i += 2, maskCounter--)
                ret |= ((mask >> maskCounter) & iin) >> i;
            return (int)ret;
        }

        public static int BytesToInt(byte[] bytes)
        {
            int ret = 0;
            for (int i = 0; i < 4; i++)
            {
                int curr = bytes[i];
                ret |= curr << (24 - (8 * i));
            }
            return ret;
        }

        public static long BytesToLong(byte[] bytes)
        {
            long ret = 0;
            for (int i = 0; i < 8; i++)
            {
                long curr = bytes[i];
                ret |= curr << (56 - (8 * i));
            }
            return ret;
        }

        public static short BytesToShort(byte[] bytes)
        {
            short ret = 0;
            for (int i = 0; i < 2; i++)
            {
                int curr = bytes[i];
                ret |= (short)(curr << (8 - (8 * i)));
            }
            return ret;
        }

        public static int ModifyIntLSB(int byteIn, bool lsbValue)
        {
            int lsb = lsbValue ? 0x1 : 0x0;
            int mask = 0xfe;
            int res = byteIn & mask;
            return res | lsb;
        }

        public static int ReadInt32(Stream streamIn)
        {
            byte[] readInt = new byte[4];
            streamIn.Read(readInt, 0, 4);
            return BytesToInt(readInt);
        }

        public static int ReadInt16(Stream streamIn)
        {
            byte[] readShort = new byte[2];
            streamIn.Read(readShort, 0, 2);
            return BytesToShort(readShort);
        }

        public static long ReadInt64(Stream streamIn)
        {
            byte[] readLong = new byte[8];
            streamIn.Read(readLong, 0, 8);
            return BytesToLong(readLong);
        }
    }
}
