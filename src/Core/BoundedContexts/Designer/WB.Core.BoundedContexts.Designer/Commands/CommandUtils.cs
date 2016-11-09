using CsQuery.ExtensionMethods.Internal;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Shared.Web.Extensions;

namespace WB.Core.BoundedContexts.Designer.Commands
{
    internal class CommandUtils
    {
        public static string SanitizeHtml(string html, bool removeAllTags = false)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            var sanitizer = new WbHtmlSanitizer();

            if (!removeAllTags)
            {
                sanitizer.AllowedTags.AddRange(new[]
                {
                    "u", "s", "i", "b", "br", "font", "tt", "big", "strong", "small", "sup", "sub", "blockquote",
                    "cite", "dfn", "p", "h1", "em"
                });
                sanitizer.AllowedAttributes.AddRange(new[] {"color", "size"});
            }
            else
            {
                sanitizer.AllowedTags.Clear();
                sanitizer.AllowedAttributes.Clear();
            }

            string sanitizedHtml = html;
            bool wasChanged = true;
            while (wasChanged)
            {
                var temp = System.Web.HttpUtility.HtmlDecode(sanitizer.Sanitize(sanitizedHtml)).Trim();
                wasChanged = sanitizedHtml != temp;
                sanitizedHtml = temp;
            }
            return sanitizedHtml;
        }
    }
}
