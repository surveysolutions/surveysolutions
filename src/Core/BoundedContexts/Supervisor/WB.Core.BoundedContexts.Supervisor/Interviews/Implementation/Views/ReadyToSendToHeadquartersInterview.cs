using System;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views
{
    internal class ReadyToSendToHeadquartersInterview : IView
    {
        public Guid InterviewId { get; private set; }

        public ReadyToSendToHeadquartersInterview(Guid interviewId)
        {
            this.InterviewId = interviewId;
        }
    }
}