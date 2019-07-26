using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSteganography
{
    namespace Steganography
    {
        public class KeySeededRng
        {
            private Random SeededRng;

            public KeySeededRng(byte[] key)
            {
                int seed = 0;
                for (int i = 0; i < key.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        seed += key[i];
                    }
                    else
                    {
                        seed ^= key[i];
                    }
                }
                SeededRng = new Random(seed);
            }

            public int Next() => SeededRng.Next();
            public int Next(int maxValue) => SeededRng.Next(maxValue);
            public int Next(int minValue, int maxValue) => SeededRng.Next(minValue, maxValue);
            public double NextDouble() => SeededRng.NextDouble();
            public void NextBytes(byte[] buffer) => SeededRng.NextBytes(buffer);
        }

        public class KeyedRng
        {
            private byte[] Key;
            private ulong InternalState = 0;
            private int CurrKeyIndex = 0;
            public KeyedRng(byte[] key)
            {
                Key = key;
            }

            private void PerformStateUpdate()
            {
                int intermediate = Key[CurrKeyIndex] * Key[(CurrKeyIndex + 1) % Key.Length];
                ulong res = (ulong)(intermediate << Key[(CurrKeyIndex + 2) % Key.Length]);
                InternalState += res;
                intermediate = Key[(CurrKeyIndex + 2) % Key.Length] * Key[CurrKeyIndex];
                InternalState ^= (ulong)intermediate;
                CurrKeyIndex += 1;
                CurrKeyIndex %= Key.Length;
                InternalState *= 3;
            }

            public int Next(int maxValue)
            {
                int ret = (int)(InternalState % (ulong)maxValue);
                PerformStateUpdate();
                return ret;
            }

        }
    }
}
