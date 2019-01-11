using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAudioAuditService
    {
        Task StartRecordingAsync(Guid interviewId);
        Task StopRecordingAsync(Guid interviewId);
    }
}
