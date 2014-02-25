using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "ReevaluateSynchronizedInterview")]
    public class ReevaluateSynchronizedInterview : CommandBase
    {
        public ReevaluateSynchronizedInterview(Guid interviewId)
            : base(interviewId)
        {
            this.InterviewId = interviewId;
        }

        [AggregateRootId]
        public Guid InterviewId { get; private set; }
    }
}
