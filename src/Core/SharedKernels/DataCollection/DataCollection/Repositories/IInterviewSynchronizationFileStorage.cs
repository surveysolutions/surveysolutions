using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IInterviewSynchronizationFileStorage
    {
        void MoveInterviewImagesToSyncFolder(Guid interviewId);
        IList<InterviewBinaryDataDescriptor> GetImagesByInterviews();
        void RemoveInterviewImage(Guid interviewId, string fileName);
    }
}
