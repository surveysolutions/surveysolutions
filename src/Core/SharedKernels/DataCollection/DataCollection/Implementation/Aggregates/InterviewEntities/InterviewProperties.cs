using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    internal class InterviewProperties
    {
        public InterviewProperties(Guid interviewerId)
        {
            this.InterviewerId = interviewerId != Guid.Empty ? interviewerId : null as Guid?;
        }

        public Guid? InterviewerId { get; }
    }
}
