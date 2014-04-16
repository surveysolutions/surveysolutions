using System;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    public class LocalInterviewFeedEntry : InterviewFeedEntry
    {
        public bool Processed { get; set; }

        public bool ProcessedWithError { get; set; }

        public Uri QuestionnaireUri { get; set; }

        public Uri InterviewUri { get; set; }
    }
}