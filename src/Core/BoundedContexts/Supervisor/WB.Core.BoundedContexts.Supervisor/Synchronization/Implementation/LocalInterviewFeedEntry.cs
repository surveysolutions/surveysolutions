using System;
using System.Diagnostics;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    [DebuggerDisplay("Processed = {Processed}; EntryType = {EntryType}; Timestamp = {Timestamp}; InterviewId = {InterviewId}")]
    public class LocalInterviewFeedEntry : InterviewFeedEntry
    {
        public virtual bool Processed { get; set; }

        public virtual bool ProcessedWithError { get; set; }

        public virtual Uri InterviewUri { get; set; }
    }
}