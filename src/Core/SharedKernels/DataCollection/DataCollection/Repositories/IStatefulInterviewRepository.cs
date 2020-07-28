#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IStatefulInterviewRepository
    {
        IStatefulInterview? Get(string interviewId);
        Task<IStatefulInterview> GetAsync(string interviewId, IProgress<EventReadingProgress> progress, CancellationToken cancellationToken);
        IStatefulInterview GetOrThrow(string interviewId);
    }
}
