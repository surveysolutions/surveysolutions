using System;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class ArrayExtensions
    {
        public static bool SequenceEqual(this decimal[] source, decimal[] target)
        {
            if (source.Length == target.Length)
            {
                for (var i = 0; i < source.Length; i++)
                {
                    if (target[i] == source[i]) continue;

                    return false;
                }

                return true;
            }

            return false;
        }

        public static bool SequenceEqual(this Guid[] source, Guid[] target)
        {
            if (source.Length == target.Length)
            {
                for (var i = 0; i < source.Length; i++)
                {
                    if (target[i] == source[i]) continue;

                    return false;
                }

                return true;
            }

            return false;
        }

        public static bool SequenceEqual<T>(this T[] source, T[] target)
        {
            if (source.Length == target.Length)
            {
                for (var i = 0; i < source.Length; i++)
                {
                    if (target[i].Equals(source[i])) continue;

                    return false;
                }

                return true;
            }

            return false;
        }
    }
}