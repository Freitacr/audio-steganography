using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography.Audio.Flac
{
    public class RiceHelper
    {

        public static int ReadFlacRiceEncodedInt(byte riceParameter, BitReader readerIn)
        {
            int divisionVal = (int)Math.Pow(2, riceParameter);
            bool readBit = readerIn.ReadBit();
            int ret = 0;
            while (!readBit)
            {
                ret += divisionVal;
                readBit = readerIn.ReadBit();
            }
            //Now we have to read in the bits that are left and put them into their places
            for (int i = riceParameter; i > 0; i--)
            {
                int res = readerIn.ReadBit() ? 0x1 : 0x0;
                res <<= i - 1;
                ret |= res;
            }
            return ret;
        }
    }
}
