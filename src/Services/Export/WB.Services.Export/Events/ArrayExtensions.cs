using System;

namespace WB.Services.Export.Events
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

        public static bool SequenceEqual(this int[] source, int[] target)
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

        public static bool SequenceEqual(this int[] source, int[] target, int targetLength)
        {
            if (source.Length == targetLength)
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

        public static bool SequenceEqual(int[] source, int sourceLength, int[] target, int targetLength)
        {
            if (sourceLength == targetLength)
            {
                for (var i = 0; i < sourceLength; i++)
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
            if (source.Length != target.Length) return false;
            
            for (var i = 0; i < source.Length; i++)
            {
                if (target[i]!.Equals(source[i])) continue;

                return false;
            }

            return true;

        }

        public static decimal[] ExtendWithOneItem(this decimal[] source, decimal coordinate)
        {
            decimal[] result;

            if (source == null || source.Length == 0)
            {
                result = new[] { coordinate };
            }
            else if (source.Length == 1)
            {
                result = new[] { source[0], coordinate };
            }
            else
            {
                result = new decimal[source.Length + 1];
                source.CopyTo(result, 0);
                result[result.Length - 1] = coordinate;
            }

            return result;
        }

        public static int[] ExtendWithOneItem(this int[] source, int coordinate)
        {
            int[] result;

            if (source == null || source.Length == 0)
            {
                result = new[] { coordinate };
            }
            else if (source.Length == 1)
            {
                result = new[] { source[0], coordinate };
            }
            else
            {
                result = new int[source.Length + 1];
                source.CopyTo(result, 0);
                result[result.Length - 1] = coordinate;
            }

            return result;
        }
    }
}
