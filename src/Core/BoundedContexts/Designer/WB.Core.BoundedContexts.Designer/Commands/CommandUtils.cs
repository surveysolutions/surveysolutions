using Html;

namespace WB.Core.BoundedContexts.Designer.Commands
{
    internal class CommandUtils
    {
        public static string SanitizeHtml(string html)
        {
            var sanitizer = new HtmlSanitizer
            {
                AllowedTags = new[] { "i", "b", "br", "font" }, 
                AllowedAttributes = new[] { "color" }
            };

            return sanitizer.Sanitize(html);
        }
    }
}
