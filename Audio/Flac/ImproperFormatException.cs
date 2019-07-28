using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography.Audio.Flac
{
    public class ImproperFormatException : Exception
    {
        public static readonly string DEFAULT_MESSAGE = "FLAC file was in improper format...";
        public ImproperFormatException() : base(DEFAULT_MESSAGE) { }
        public ImproperFormatException(string message) : base(message) { }
    }
}
