using System;

namespace WB.Core.SharedKernels.DataCollection.Services
{
    public interface IInterviewBinaryCleanupService
    {
        void EnqueueImageCleanup(Guid interviewId, string fileName, Exception exception);
        void EnqueueAudioCleanup(Guid interviewId, string fileName, Exception exception);
        void ProcessPending(Guid interviewId);
    }

    public class NullInterviewBinaryCleanupService : IInterviewBinaryCleanupService
    {
        public void EnqueueImageCleanup(Guid interviewId, string fileName, Exception exception)
        {
        }

        public void EnqueueAudioCleanup(Guid interviewId, string fileName, Exception exception)
        {
        }

        public void ProcessPending(Guid interviewId)
        {
        }
    }
}
