using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace WB.Tests.Unit
{
    internal static class EnumerableExtensions
    {
        public static T Second<T>(this IEnumerable<T> enumerable) => enumerable.Skip(1).First();

        public static void SequenceEqual<TSource, TTarget, TProp>(
            this IEnumerable<TSource> source, IEnumerable<TTarget> target, 
            Func<TSource, TProp> sourceProp, Func<TTarget, TProp> targetProp)
        {
            var sourceArray = source.Select(sourceProp).ToArray();
            var targetArray = target.Select(targetProp).ToArray();

            sourceArray.AssertThatSequenceEqual(targetArray);
        }

        internal static bool AssertThatSequenceEqual<T>(this T[] source, T[] target)
        {
            if (source.Length == target.Length)
            {
                for (var i = 0; i < source.Length; i++)
                {
                    Assert.That(target[i], Is.EqualTo(source[i]));
                }

                return true;
            }

            return false;
        }

    }
}