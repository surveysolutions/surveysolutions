using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class AllInterviewsViewItem : BaseInterviewGridItem
    {
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanUnapprove { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { set; get; }

        public bool ReceivedByInterviewer { get; set; }
    }
}