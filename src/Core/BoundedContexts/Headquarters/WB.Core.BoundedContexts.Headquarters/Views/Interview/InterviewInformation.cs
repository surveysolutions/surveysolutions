using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

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
    }
}
