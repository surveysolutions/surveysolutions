using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class InterviewApiView
    {
        public Guid Id { get; set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
        public bool IsRejected { get; set; }
        public Guid ResponsibleId { get; set; }
        public int? Sequence { get; set; }
        public bool IsMarkedAsReceivedByInterviewer { get; set; }
    }
}
