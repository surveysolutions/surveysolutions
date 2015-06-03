using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    internal class LimitlessInterviewPreconditionsService : IInterviewPreconditionsService
    {
        public long? InterviewCountLimit
        {
            get { return null; }
        }

        public bool IsInterviewCountLimitReached()
        {
            return false;
        }
    }
}
