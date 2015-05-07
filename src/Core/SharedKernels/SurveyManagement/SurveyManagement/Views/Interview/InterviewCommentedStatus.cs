using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewCommentedStatus
    {
        public virtual string Comment { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual InterviewStatus Status { get; set; }
        public virtual string Responsible { get; set; }
        public virtual Guid ResponsibleId { get; set; }
    }
}