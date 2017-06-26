using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.Services
{
    public class AssignmentImportStatus
    {
        public AssignmentImportStatus()
        {
            this.State = new InterviewImportState { Errors = new List<InterviewImportError>() };
        }

        public string InterviewImportProcessId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string QuestionnaireTitle { get; set; }
        public bool IsInProgress { get; set; } = false;
        public DateTime StartedDateTime { get; set; }
        public int TotalInterviewsCount { get; set; }
        public int CreatedInterviewsCount { get; set; }
        public double TimePerInterview { get; set; }
        public double ElapsedTime { get; set; }
        public double EstimatedTime { get; set; }
        public InterviewImportState State { get; set; }
        public PreloadedContentType PreloadedContentType { get; set; }
    }
}