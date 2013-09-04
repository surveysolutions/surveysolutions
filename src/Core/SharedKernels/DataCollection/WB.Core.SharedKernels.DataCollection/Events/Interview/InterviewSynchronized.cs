using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class InterviewSynchronized : InterviewActiveEvent
    {
        public InterviewSynchronized(InterviewSynchronizationDto interviewData)
            : base(interviewData.UserId)
        {
            InterviewData = interviewData;
        }

        public InterviewSynchronizationDto InterviewData { get; private set; }
    }
}
