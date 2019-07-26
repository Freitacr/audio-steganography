using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography
{
    namespace Helper
    {
        public class KeyHelper
        {
            private static byte[] ConvertStringToBytes(string input)
            {
                List<byte> ret = new List<byte>();
                for (int i = 0; i < input.Length; i++)
                    ret.Add((byte)input[i]);
                return ret.ToArray();
            }

            public static byte[] GenerateKeyFromUserInput()
            {
                PasswordEntryForm entryForm = new PasswordEntryForm();
                while (entryForm.Password1 == "" || entryForm.Password2 == "")
                    entryForm.ShowDialog();
                SHA256 sha = SHA256.Create();
                List<byte> ret = new List<byte>();
                var p1Hash = sha.ComputeHash(ConvertStringToBytes(entryForm.Password1));
                var p2Hash = sha.ComputeHash(ConvertStringToBytes(entryForm.Password2));
                for (int i = 0; i < p1Hash.Length; i++)
                {
                    ret.Add(p1Hash[i]);
                    ret.Add(p2Hash[i]);
                }
                return ret.ToArray();
            }
        }
    }
}
