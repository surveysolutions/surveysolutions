using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewInvariants
    {
        public InterviewInvariants(InterviewEntities.InterviewProperties properties)
        {
            this.Properties = new InterviewPropertiesInvariants(properties);
        }

        public InterviewPropertiesInvariants Properties { get; }
    }
}
