using System;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewData : IView
    {
        public Guid InterviewId { get; set; }

        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public InterviewStatus Status { get; set; }

        public Guid ResponsibleId { get; set; }
        public UserRoles ResponsibleRole { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}