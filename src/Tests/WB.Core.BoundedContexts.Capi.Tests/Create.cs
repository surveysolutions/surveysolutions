using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Tests
{
    internal static class Create
    {
        public static InterviewItemId InterviewItemId(Guid id, decimal[] rosterVector = null)
        {
            return new InterviewItemId(id, rosterVector);
        }
    }
}
