using System;
using CsQuery;
using CsQuery.ExtensionMethods;
using Html;

namespace WB.Core.BoundedContexts.Designer.Commands
{
    internal class WbHtmlSanitizer : HtmlSanitizer
    {
        protected override void OnRemovingTag(RemovingTagEventArgs e)
        {
            var tag = e.Tag;
            var parentNode = tag.ParentNode;
            if (parentNode.Document == null) return;
            string innerText = System.Web.HttpUtility.HtmlDecode(tag.InnerText.Replace("&#65533;",""));
            string innerHtml = System.Web.HttpUtility.HtmlDecode(tag.InnerHTML.Replace("&#65533;",""));
            if (!string.IsNullOrWhiteSpace(innerText) && innerText == innerHtml)
            {
                var text = parentNode.Document.CreateTextNode(innerText);
                parentNode.InsertBefore(text, tag);
            }

            if (innerText != innerHtml)
            {
                if (!string.IsNullOrWhiteSpace(innerText))
                {
                    parentNode.InsertBefore(parentNode.Document.CreateTextNode(innerText), tag);
                }
                foreach (var child in tag.ChildElements )
                {
                    if (child != null)
                    {
                        parentNode.InsertBefore(child.Clone(), tag);
                        parentNode.InsertBefore(parentNode.Document.CreateTextNode(" "), tag);
                    }
                }
              
            }
        }
    }

    internal class CommandUtils
    {
        public static string SanitizeHtml(string html, bool removeAllTags = false)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            var sanitizer = new WbHtmlSanitizer
            {
                AllowedTags = removeAllTags ? new string[0] : new[] { "u", "s", "i", "b", "br", "font" },
                AllowedAttributes = removeAllTags ? new string[0] : new[] { "color" }
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
