using System;
using System.Diagnostics;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Crypto
{
    [DebuggerStepThrough]
    internal static class ArrayExtensions
    {
        public static void Clear<T>(this T[] a_array, T a_value = default(T))
        {
            for (int i = 0; i < a_array.Length; i++)
                a_array[i] = a_value;
        }

        public static void Clear<T>(this T[,] a_array, T a_value = default(T))
        {
            for (int x = 0; x < a_array.GetLength(0); x++)
            {
                for (int y = 0; y < a_array.GetLength(1); y++)
                {
                    a_array[x, y] = a_value;
                }
            }
        }

        public static T[] SubArray<T>(this T[] a_array, int a_index, int a_count)
        {
            T[] result = new T[a_count];
            Array.Copy(a_array, a_index, result, 0, a_count);
            return result;
        }
    }
}