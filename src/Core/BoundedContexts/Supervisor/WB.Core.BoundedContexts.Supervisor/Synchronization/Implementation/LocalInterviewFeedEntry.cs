using System;
using System.Diagnostics;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    [DebuggerDisplay("Processed = {Processed}; EntryType = {EntryType}; Timestamp = {Timestamp}; InterviewId = {InterviewId}")]
    public class LocalInterviewFeedEntry : InterviewFeedEntry
    {
        public bool Processed { get; set; }

        public bool ProcessedWithError { get; set; }

        public Uri InterviewUri { get; set; }
    }
}