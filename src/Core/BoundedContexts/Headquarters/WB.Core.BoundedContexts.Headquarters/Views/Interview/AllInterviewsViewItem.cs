using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class AllInterviewsViewItem : BaseInterviewGridItem
    {
        public bool CanBeReassigned { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanUnapprove { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { set; get; }
        public string Key { get; set; }
        public bool ReceivedByInterviewer { get; set; }
        public string TeamLeadName { get; set; }
        public string ClientKey { get; set; }
    }

    public class InterviewListItem
    {
        public Guid InterviewId { get; set; }
        public string Key { get; set; }
        public string QuestionnaireId { get; set; }
        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public UserRoles ResponsibleRole { get; set; }
        public Guid TeamLeadId { get; set; }
        public string TeamLeadName { get; set; }
        public InterviewStatus Status { get; set; }
        public string UpdateDate { get; set; }
        public bool WasCreatedOnClient { get; set; }
        public bool ReceivedByInterviewer { get; set; }
    }
}
