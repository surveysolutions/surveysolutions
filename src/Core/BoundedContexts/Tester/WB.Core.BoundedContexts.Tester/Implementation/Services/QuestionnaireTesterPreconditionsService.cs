using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    internal class QuestionnaireTesterPreconditionsService : IInterviewPreconditionsService
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
