using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewInformation
    {
        public Guid Id { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public bool IsRejected { get; set; }
        public Guid ResponsibleId { get; set; }
        public int? LastEventSequence { get; set; }
        public bool IsReceivedByInterviewer { get; set; }
        public Guid? LastEventId { get; set; }
        public InterviewMode Mode { get;set; } 
    }
}
