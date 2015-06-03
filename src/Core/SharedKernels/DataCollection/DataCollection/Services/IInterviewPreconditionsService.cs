using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewPreconditionsService
    {
        long? InterviewCountLimit { get; }
        bool IsInterviewCountLimitReached();
    }
}
