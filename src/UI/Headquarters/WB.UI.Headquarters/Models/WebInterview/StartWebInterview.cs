using System;
using System.Collections.Generic;
using System.Web;
using WB.Core.BoundedContexts.Headquarters.WebInterview;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class ResumeWebInterview
    {
        public string QuestionnaireTitle { get; set; }
        public bool UseCaptcha { get; set; }
        public DateTime? StartedDate { get; set; }
        public Dictionary<WebInterviewUserMessages, string> CustomMessages { get; set; }
    }

    public class StartWebInterview
    {
        public string QuestionnaireTitle { get; set; }
        public bool UseCaptcha { get; set; }
        public bool ServerUnderLoad { get; set; } = false;
        public Dictionary<WebInterviewUserMessages, string> CustomMessages { get; set; }
    }

    public class FinishWebInterview
    {
        public string QuestionnaireTitle { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public Dictionary<WebInterviewUserMessages, string> CustomMessages { get; set; }
    }

    public class WebInterviewError
    {
        public string Message { get; set; }
    }

    public static class WebInterviewMsgExtensions
    {
        public static IHtmlString GetText(this Dictionary<WebInterviewUserMessages, string> messages,
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