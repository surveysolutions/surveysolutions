using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    public class DummyPreconditionsService : IInterviewPreconditionsService
    {
        public int? GetMaxAllowedInterviewsCount()
        {
            return null;
        }

        public int? GetInterviewsCountAllowedToCreateUntilLimitReached()
        {
            return null;
        }
    }
}
