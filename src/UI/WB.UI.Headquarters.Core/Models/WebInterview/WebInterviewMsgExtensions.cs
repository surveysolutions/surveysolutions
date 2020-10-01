#nullable enable
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using WB.Core.BoundedContexts.Headquarters.WebInterview;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public static class WebInterviewMsgExtensions
    {
        public static IHtmlContent GetText(this Dictionary<WebInterviewUserMessages, string>? messages,
            WebInterviewUserMessages message)
        {
            if (messages == null || !messages.ContainsKey(message) || string.IsNullOrEmpty(messages[message]))
            {
                return new HtmlString(WebInterviewConfig.DefaultMessages[message]);
            }

            return new HtmlString(messages[message]);
        }
    }
}
