using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Aggregates;

namespace WB.Core.SharedKernels.Enumerator.Repositories
{
    public interface IStatefulInterviewRepository
    {
        IStatefulInterview Get(string interviewId);
        Task<IStatefulInterview> GetAsync(string interviewId, IProgress<int> progress, CancellationToken cancellationToken);
    }
}