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
    }

    public class WebInterviewError
    {
        public string Message { get; set; }
    }
}