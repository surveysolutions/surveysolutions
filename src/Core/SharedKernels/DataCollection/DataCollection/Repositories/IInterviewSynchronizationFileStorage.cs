using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IInterviewSynchronizationFileStorage
    {
        void MoveInterviewsBinaryDataToSyncFolder(Guid interviewId);
        IList<InterviewBinaryDataDescriptor> GetBinaryFilesFromSyncFolder();
        void RemoveBinaryDataFromSyncFolder(Guid interviewId, string fileName);
    }
}
