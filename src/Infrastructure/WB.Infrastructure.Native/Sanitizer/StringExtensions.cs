namespace WB.Infrastructure.Native.Sanitizer
{
    public static class StringExtensions
    {
        public static string RemoveHtmlTags(this string value)
        {
            if (value == null) return null;
            var sanitizer = new WbHtmlSanitizer();
            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedTags.Clear();

            return sanitizer.Sanitize(value);
        }
    }
}