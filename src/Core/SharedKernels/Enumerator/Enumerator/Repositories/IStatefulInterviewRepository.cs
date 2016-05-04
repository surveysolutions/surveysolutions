using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Aggregates;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface IStatefulInterviewRepository
    {
        IStatefulInterview Get(string interviewId);
    }
}