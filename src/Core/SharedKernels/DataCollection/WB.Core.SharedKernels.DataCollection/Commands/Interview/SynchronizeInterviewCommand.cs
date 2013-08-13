using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "SynchronizeInterview")]
    public class SynchronizeInterviewCommand : InterviewCommand
    {
        public SynchronizeInterviewCommand(Guid interviewId, Guid userId, InterviewSynchronizationDto sycnhronizedInterview) : base(interviewId, userId)
        {
            SynchronizedInterview = sycnhronizedInterview;
        }

        public InterviewSynchronizationDto SynchronizedInterview { get; private set; }
    }
}
