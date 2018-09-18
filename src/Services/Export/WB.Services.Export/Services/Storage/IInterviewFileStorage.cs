using System;
using System.Collections.Generic;

namespace WB.Services.Export.Services.Storage
{
    public interface IInterviewFileStorage
    {
        byte[] GetInterviewBinaryData(Guid interviewId, string fileName);
        List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId);
        void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType);
        void RemoveInterviewBinaryData(Guid interviewId, string fileName);
    }
}