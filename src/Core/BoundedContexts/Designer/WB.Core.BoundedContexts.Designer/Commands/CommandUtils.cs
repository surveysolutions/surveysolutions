using System;
using Ganss.Xss;

namespace WB.Core.BoundedContexts.Designer.Commands
{
    internal class CommandUtils
    {
        static string[] AllowedTags = new string[]
        {
            "u", "s", "i", "b", "br", "font", "tt", "big", "strong", "small", "sup", "sub", "blockquote",
                "cite", "dfn", "p", "em"
        };

        static string[] AllowedAttributes = new string[]
        {
            "color", "size"
        };

        public static string SanitizeHtml(string? html, bool removeAllTags = false)
        {
            if (string.IsNullOrWhiteSpace(html))
                return String.Empty;

            var sanitizer = new HtmlSanitizer { KeepChildNodes = true };

            if (!removeAllTags)
            {
                sanitizer.AllowedTags.Clear();
                foreach (var tag in AllowedTags)
                {
                    sanitizer.AllowedTags.Add(tag);
                }

                sanitizer.AllowedAttributes.Clear();
                foreach (var attribute in AllowedAttributes)
                {
                    sanitizer.AllowedAttributes.Add(attribute);
                }
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
