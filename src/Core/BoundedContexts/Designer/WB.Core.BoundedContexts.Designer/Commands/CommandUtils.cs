using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Core.BoundedContexts.Designer.Commands
{
    internal class CommandUtils
    {
        public static string SanitizeHtml(string html, bool removeAllTags = false)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            var sanitizer = new WbHtmlSanitizer
            {
                AllowedTags = removeAllTags ? new string[0] : new[] { "u", "s", "i", "b", "br", "font", "tt", "big", "strong", "small", "sup" ,"sub", "blockquote", "cite", "dfn", "p", "h1", "em"},
                AllowedAttributes = removeAllTags ? new string[0] : new[] { "color", "size" }
            };
            string sanitizedHtml = html;
            bool wasChanged = true;
            while (wasChanged)
            {
                var temp = System.Web.HttpUtility.HtmlDecode(sanitizer.Sanitize(sanitizedHtml)).Trim();
                wasChanged = (sanitizedHtml != temp);
                sanitizedHtml = temp;
            }
            return sanitizedHtml;
        }
    }
}
