using System;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views
{
    public class ReadyToSendToHeadquartersInterview : IView
    {
        protected ReadyToSendToHeadquartersInterview() {}

        public ReadyToSendToHeadquartersInterview(Guid interviewId)
        {
            this.InterviewId = interviewId;
            this.Id = interviewId.FormatGuid();
        }

        public virtual string Id { get; set; }

        public virtual Guid InterviewId { get; protected set; }
    }
}