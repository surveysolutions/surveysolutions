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

        public static int GenerateHash(Guid id)
        {
            //hash 
            //return this._a ^ (this._b << 16 | (ushort)this._c) ^ (this._f << 24 | this._k);

            //return this._a ^ *Unsafe.Add<int>(ref this._a, 1) ^ *Unsafe.Add<int>(ref this._a, 2) ^ *Unsafe.Add<int>(ref this._a, 3);

            var b = id.ToByteArray();

            int _a = (int)b[3] << 24 | (int)b[2] << 16 | (int)b[1] << 8 | (int)b[0];
            short _b = (short)((int)b[5] << 8 | (int)b[4]);
            short _c = (short)((int)b[7] << 8 | (int)b[6]);
            //var _d = b[8];
            //var _e = b[9];
            var _f = b[10];
            //var _g = b[11];
            //var _h = b[12];
            //var _i = b[13];
            //var _j = b[14];
            var _k = b[15];


            int result = _a ^ (_b | _c) ^ (_f | _k);

            //int result = _a ^ *Unsafe.Add<int>(ref _a, 1) ^ *Unsafe.Add<int>(ref _a, 2) ^ *Unsafe.Add<int>(ref _a, 3);
            return result;
        }

        /*internal static class Unsafe
        {
            public unsafe static T* Add<T>(ref T source, int elementOffset)
            {
                return ref source + (IntPtr) elementOffset * (IntPtr) sizeof(T);
            }
        }*/
    }
}
