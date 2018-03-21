using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewItemIdTests
{
    [NUnit.Framework.TestOf(typeof(InterviewItemId))]
    internal class InterviewItemIdTestsContext
    {
        protected static InterviewItemId CreateInterviewItemId(Guid id, decimal[] propagationVector)
        {
            return new InterviewItemId(id, propagationVector);
        }
    }
}
