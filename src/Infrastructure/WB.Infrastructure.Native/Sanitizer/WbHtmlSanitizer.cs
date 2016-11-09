using System.Web;
using Ganss.XSS;

namespace WB.Infrastructure.Native.Sanitizer
{
    public class WbHtmlSanitizer : HtmlSanitizer
    {
        protected override void OnRemovingTag(RemovingTagEventArgs e)
        {
            var tag = e.Tag;
            var parentNode = tag.Parent;
            if (parentNode.Owner == null) return;

            string innerText = HttpUtility.HtmlDecode(tag.TextContent.Replace("&#65533;",""));
            string innerHtml = HttpUtility.HtmlDecode(tag.InnerHtml.Replace("&#65533;",""));
            if (!string.IsNullOrWhiteSpace(innerText) && innerText == innerHtml)
            {
                var text = parentNode.Owner.CreateTextNode(innerText);
                parentNode.InsertBefore(text, tag);
            }

            if (innerText != innerHtml)
            {
                if (!string.IsNullOrWhiteSpace(innerText))
                {
                    parentNode.InsertBefore(parentNode.Owner.CreateTextNode(innerText), tag);
                }
                foreach (var child in tag.ChildNodes )
                {
                    if (child != null)
                    {
                        parentNode.InsertBefore(child.Clone(), tag);
                        parentNode.InsertBefore(parentNode.Owner.CreateTextNode(" "), tag);
                    }
                }
              
            }
        }
    }
}