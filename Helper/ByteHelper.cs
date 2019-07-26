using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
