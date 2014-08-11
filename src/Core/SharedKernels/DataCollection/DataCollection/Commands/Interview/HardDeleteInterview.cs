using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "HardDelete")]
    public class HardDeleteInterview : InterviewCommand
    {
        public HardDeleteInterview(Guid interviewId, Guid userId)
            : base(interviewId, userId) {}
    }
}
