using System;

namespace WB.Enumerator.Native.WebInterview.Models
{
    public class ResumeWebInterview
    {
        public string QuestionnaireTitle { get; set; }
        public bool UseCaptcha { get; set; }
        public DateTime? StartedDate { get; set; }
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
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class WebInterviewError
    {
        public string Message { get; set; }
    }
}