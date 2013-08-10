using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.ValueObjects.Interview
{
    public enum InterviewStatus
    {
        Created,
        SupervisorAssigned,
        InterviewerAssigned,
        ReadyForInterview, // not used so far
        SentToCapi, // not used so far
        Complete,
        Restarted,
        ApprovedBySupervisor,
        RejectedBySupervisor,
    }
}
