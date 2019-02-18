using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IAudioAuditService
    {
        Task StartAudioRecordingAsync(Guid interviewId);
        Task StopAudioRecordingAsync(Guid interviewId);

        void CheckAndProcessAllAuditFiles();
    }
}
