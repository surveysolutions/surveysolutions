using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAuditService
    {
        Task StartAudioRecordingAsync(Guid interviewId);
        Task StopAudioRecordingAsync(Guid interviewId);
    }
}
