using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNetCore.Html;
using WB.Core.BoundedContexts.Headquarters.WebInterview;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class StartWebInterview : StartOrResumeWebInterview
    {
        public string WelcomeText { get; set; }

        public string Description { get; set; }
        public string StartNewButton { get; set; }
        public string ResumeButton { get; set; }
    }

    public class ResumeWebInterview : StartOrResumeWebInterview
    {
        public string StartedDate { get; set; }
        public string ResumeWelcome { get; set; }
        public string ResumeInvitation { get; set; }
        public string ResumeButton { get; set; }
    }

    public class StartOrResumeWebInterview
    {
        public string RecaptchaSiteKey { get; set; }
        public string QuestionnaireTitle { get; set; }
        public bool UseCaptcha { get; set; }
        public bool ServerUnderLoad { get; set; } = false;
        public bool HasPassword { get; set; }
        public bool HasPendingInterviewId { get; set; }
        public List<string> CaptchaErrors { get; set; }
        public bool IsPasswordInvalid { get; set; }
        public string SubmitUrl { get; set; }
        public string HostedCaptchaHtml { get; set; }
    }

    public class FinishWebInterview
    {
        public string QuestionnaireTitle { get; set; }
        public string StartedDate { get; set; }
        public string CompletedDate { get; set; }
        public string WebSurveyHeader { get; set; }
        public string FinishInterview { get; set; }
        public string SurveyName { get; set; }
        public string PrintUrl { get; set; }
    }

    public class WebInterviewError
    {
        public string ErrorMessage { get; set; }
        public bool AllowInterviewRestart { get; set; }
    }

    public static class WebInterviewMsgExtensions
    {
        public static IHtmlContent GetText(this Dictionary<WebInterviewUserMessages, string> messages,
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
