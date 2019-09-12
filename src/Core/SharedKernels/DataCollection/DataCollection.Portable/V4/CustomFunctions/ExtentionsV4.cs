using System;

namespace WB.Core.SharedKernels.DataCollection.V4.CustomFunctions
{
    public static class ExtentionsV4
    {
        public static double GetRandomDouble(this Guid id)
        {
            var hash = GenerateHash(id);
            Random r = new Random(hash);
            return r.NextDouble();
        }

        private static int GenerateHash(Guid id)
        {
            //hash from .net
            //return this._a ^ (this._b << 16 | (ushort)this._c) ^ (this._f << 24 | this._k);

            //portable
            //return _a ^ Unsafe.Add(ref _a, 1) ^ Unsafe.Add(ref _a, 2) ^ Unsafe.Add(ref _a, 3);

            var b = id.ToByteArray();

            return CalculateHashAsPortableNet(b);
        }

        private static int CalculateHashAsNet(byte[] b)
        {
            int _a = (int)b[3] << 24 | (int)b[2] << 16 | (int)b[1] << 8 | (int)b[0];

            short _b = (short)((int)b[5] << 8 | (int)b[4]);
            short _c = (short)((int)b[7] << 8 | (int)b[6]);
            var _f = b[10];
            var _k = b[15];

            int result = _a ^ (_b | _c) ^ (_f | _k);
            return result;
        }

        private static int CalculateHashAsPortableNet(byte[] b)
        {
            int[] ints = new int[4];
            for (int i = 0; i < 4; i++)
            {
                ints[i] = BitConverter.ToInt32(b, i * 4);
            }

            var result = ints[0] ^ ints[1] ^ ints[2] ^ ints[3];
            return result;
        }
    }
}
