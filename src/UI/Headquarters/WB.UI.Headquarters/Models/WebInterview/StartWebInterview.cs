using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WB.UI.Headquarters.Models.WebInterview
{
    public class ResumeWebInterview
    {
        public string QuestionnaireTitle { get; set; }
        public bool UseCaptcha { get; set; }
    }

    public class StartWebInterview
    {
        public string QuestionnaireTitle { get; set; }
        public bool UseCaptcha { get; set; }
        public bool ServerUnderLoad { get; set; } = false;
    }

    public class FinishWebInterview
    {
        public string QuestionnaireTitle { get; set; }
        public string InterviewId { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class WebInterviewError
    {
        public string Message { get; set; }
    }
}