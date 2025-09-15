using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IInterviewFileStorage
    {
        Task<byte[]> GetInterviewBinaryDataAsync(Guid interviewId, string fileName);
        byte[] GetInterviewBinaryData(Guid interviewId, string fileName);
        Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId);
        void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType);
        void StoreBrokenInterviewBinaryData(Guid userId, Guid interviewId, string fileName, byte[] data, string contentType);
        Task RemoveInterviewBinaryData(Guid interviewId, string fileName);
    }
}
