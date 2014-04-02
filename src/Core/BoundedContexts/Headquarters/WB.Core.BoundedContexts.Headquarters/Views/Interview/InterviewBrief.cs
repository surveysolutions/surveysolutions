using System;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewBrief : IView
    {
        public Guid InterviewId { get; set; }
        
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public Guid ResponsibleId { get; set; }
        public InterviewStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public bool HasErrors { get; set; }
    }
}