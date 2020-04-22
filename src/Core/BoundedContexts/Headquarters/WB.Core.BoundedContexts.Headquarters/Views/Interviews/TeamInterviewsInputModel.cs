using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interviews
{
    public class TeamInterviewsInputModel: ListViewModelBase
    {
        public Guid? QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }
        public string ResponsibleName { get; set; }
        public Guid? InterviewId { get; set; }
        public InterviewStatus[] Statuses { get; set; }
        public bool ReceivedByInterviewer { get; set; }
        public Guid? ViewerId { get; set; }
        public string SearchBy { get; set; }
        public int? AssignmentId { get; set; }
        public DateTime? UnactiveDateStart { get; set; }
        public DateTime? UnactiveDateEnd { get; set; }
    }
}
