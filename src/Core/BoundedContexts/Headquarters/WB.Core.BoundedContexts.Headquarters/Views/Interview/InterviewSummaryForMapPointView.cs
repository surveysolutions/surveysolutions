using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewSummaryForMapPointView
    {
        public string InterviewerName { get; set; }
        public string SupervisorName { get; set; }
        public string LastStatus { get; set; }
        public string LastUpdatedDate { get; set; }
        public string InterviewKey { get; set; }
        public int? AssignmentId { get; set; }
        public Guid InterviewId { get; set; }
        public List<AnswerView> IdentifyingData { get; set; }
        public InterviewStatus Status { get; set; }
        public bool IsReceivedByTablet { get; set; }
    }
}
