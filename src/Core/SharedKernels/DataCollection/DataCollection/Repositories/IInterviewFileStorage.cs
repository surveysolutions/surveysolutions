using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IInterviewFileStorage
    {
        byte[] GetInterviewBinaryData(Guid interviewId, string fileName);
        List<InterviewBinaryDataDescriptor> GetBinaryFilesForInterview(Guid interviewId);
        void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data, string contentType);
        void RemoveInterviewBinaryData(Guid interviewId, string fileName);
    }
}
