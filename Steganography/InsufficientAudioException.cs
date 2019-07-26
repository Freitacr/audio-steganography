using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography
{
    namespace Steganography
    {
        public class InsufficientAudioException : Exception
        {
            private static readonly string DEFAULT_MESSAGE = "There is not enough audio data to support valid steganography of the selected data." +
                "\nConsider adding more audio data (a longer audio file, or an audio file with a higher sample rate), or reducing the amount of data to hide.";

            public InsufficientAudioException(string message = null) : base(message ?? DEFAULT_MESSAGE) { }
        }
    }
}
