using System;

namespace WB.UI.Headquarters.Controllers
{
    public class InterviewImportStatus
    {
        public string QuestionnaireTitle { get; set; }
        public bool IsInProgress { get; set; } = false;
        public DateTime StartedDateTime { get; set; }
        public int TotalInterviewsCount { get; set; }
        public int CreatedInterviewsCount { get; set; }
        public double TimePerInterview { get; set; }
        public double ElapsedTime { get; set; }
        public double EstimatedTime { get; set; }
    }
}