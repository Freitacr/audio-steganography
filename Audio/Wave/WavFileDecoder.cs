using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography.Audio.Wave
{
    public class WaveFileDecoder
    {
        private WaveFileHeader Header_;
        public WaveFileHeader FileHeader { get { return Header_; } }
        private Stream FileInputStream;
        public WaveFileDecoder(string fileName)
        {
            FileInputStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        public void DecodeFile(Action<byte[]> blockDecodeCallback)
        {
            Header_ = new WaveFileHeader(FileInputStream);
            Header_.ParseWaveHeader();
        }
    }
}
