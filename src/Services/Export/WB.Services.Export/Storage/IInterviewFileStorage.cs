using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Services.Export.Storage
{
    public interface IInterviewFileStorage
    {
        Task<byte[]> GetInterviewBinaryData(Guid interviewId, string fileName);
        Task<List<InterviewBinaryDataDescriptor>> GetBinaryFilesForInterview(Guid interviewId);
        Task StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType);
        Task RemoveInterviewBinaryData(Guid interviewId, string fileName);
    }
}
