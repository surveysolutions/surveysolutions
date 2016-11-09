using Ganss.XSS;

namespace WB.Infrastructure.Native.Sanitizer
{
    public static class StringExtensions
    {
        public static string RemoveHtmlTags(this string value)
        {
            if (value == null) return null;
            var sanitizer = new HtmlSanitizer { KeepChildNodes = true };
            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedTags.Clear();

            return sanitizer.Sanitize(value);
        }
    }
}