﻿using System.Collections.Generic;
using System.Linq;

namespace WB.Tests.Unit
{
    internal static class EnumerableExtensions
    {
        public static T Second<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Skip(1).First();
        }
    }
}