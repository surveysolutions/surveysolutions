using Ganss.Xss;

namespace WB.Infrastructure.Native.Sanitizer
{
    public static class StringExtensions
    {
        private static readonly HtmlSanitizer sanitizer;

        static StringExtensions()
        {
            sanitizer = new HtmlSanitizer { KeepChildNodes = true };
            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedTags.Clear();
        }

        public static string RemoveHtmlTags(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return sanitizer.Sanitize(value);
        }
    }
}
