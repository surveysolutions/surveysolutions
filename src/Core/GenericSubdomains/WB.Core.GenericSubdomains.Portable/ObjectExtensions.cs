using System;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class ObjectExtensions
    {
        public static string AsCompositeKey(object key, object value)
        {
            return string.Format("{0}${1}", key, value);
        }

        public static Lazy<T> ToInitializedLazy<T>(this T value)
        {
            var lazy = new Lazy<T>(() => value);

            var touch = lazy.Value;

            return lazy;
        }
    }
}
