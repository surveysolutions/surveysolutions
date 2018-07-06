using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IOfflineSynchronizationService
    {
        Task SynchronizeAsync(string endpoint, IProgress<OfflineSynchronizationProgress> progress);
    }

    public class OfflineSynchronizationProgress
    {
    }
}
