using System;
using System.Runtime.CompilerServices;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class ArrayExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual(this decimal[] source, decimal[] target)
        {
            if (source.Length != target.Length) return false;
            
            switch (source.Length)
            {
                case 1:
                    return source[0] == target[0];
                case 2:
                    return source[0] == target[0] && source[1] == target[1];
                case 3:
                    return source[0] == target[0] && source[1] == target[1] && source[2] == target[2];
                default:
                    for (var i = 0; i < source.Length; i++)
                    {
                        if (target[i] == source[i]) continue;

                        return false;
                    }
                    return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual(this int[] source, int[] target)
        {
            if (source.Length != target.Length) return false;
            
            switch (source.Length)
            {
                case 1:
                    return source[0] == target[0];
                case 2:
                    return source[0] == target[0] && source[1] == target[1];
                case 3:
                    return source[0] == target[0] && source[1] == target[1] && source[2] == target[2];
                default:
                    for (var i = 0; i < source.Length; i++)
                    {
                        if (target[i] == source[i]) continue;

                        return false;
                    }
                    return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual(this int[] source, int[] target, int targetLength)
        {
            if (source.Length != targetLength || target.Length < targetLength) return false;
            
            switch (targetLength)
            {
                case 1:
                    return source[0] == target[0];
                case 2:
                    return source[0] == target[0] && source[1] == target[1];
                case 3:
                    return source[0] == target[0] && source[1] == target[1] && source[2] == target[2];
                default:
                    for (var i = 0; i < targetLength; i++)
                    {
                        if (target[i] == source[i]) continue;

                        return false;
                    }
                    return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual(int[] source, int sourceLength, int[] target, int targetLength)
        {
            if (sourceLength != targetLength) return false;
            
            switch (sourceLength)
            {
                case 1:
                    return source[0] == target[0];
                case 2:
                    return source[0] == target[0] && source[1] == target[1];
                case 3:
                    return source[0] == target[0] && source[1] == target[1] && source[2] == target[2];
                default:
                    for (var i = 0; i < sourceLength; i++)
                    {
                        if (target[i] == source[i]) continue;

                        return false;
                    }
                    return true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual(this long[] source, int sourceLength, long[] target, int targetLength)
        {
            if (sourceLength == targetLength)
            {
                switch (sourceLength)
                {
                    case 1:
                        return source[0] == target[0];
                    case 2:
                        return source[0] == target[0] && source[1] == target[1];
                    case 3:
                        return source[0] == target[0] && source[1] == target[1] && source[2] == target[2];
                    default:
                        for (var i = 0; i < sourceLength; i++)
                        {
                            if (target[i] == source[i]) continue;

                            return false;
                        }
                        return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual(this long[] source, long[] target)
        {
            if (source.Length == target.Length)
            {
                switch (source.Length)
                {
                    case 1:
                        return source[0] == target[0];
                    case 2:
                        return source[0] == target[0] && source[1] == target[1];
                    case 3:
                        return source[0] == target[0] && source[1] == target[1] && source[2] == target[2];
                    default:
                        for (var i = 0; i < source.Length; i++)
                        {
                            if (target[i] == source[i]) continue;

                            return false;
                        }
                        return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SequenceEqual<T>(this T[] source, T[] target)
        {
            if (source is long[] longSource && target is long[] longTarget)
            {
                return SequenceEqual(longSource, longTarget);
            }

            if (source.Length == target.Length)
            {
                switch (source.Length)
                {
                    case 1:
                        return source[0].Equals(target[0]);
                    case 2:
                        return source[0].Equals(target[0]) && source[1].Equals(target[1]);
                    case 3:
                        return source[0].Equals(target[0]) && source[1].Equals(target[1]) && source[2].Equals(target[2]);
                    default:
                        for (var i = 0; i < source.Length; i++)
                        {
                            if (target[i].Equals(source[i])) continue;

                            return false;
                        }
                        return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
