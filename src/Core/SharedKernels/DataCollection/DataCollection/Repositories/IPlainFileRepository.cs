using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IPlainFileRepository
    {
        byte[] GetInterviewBinaryData(Guid interviewId, string fileName);
        void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data);
        void RemoveInterviewBinaryData(Guid interviewId, string fileName);
        void RemoveAllBinaryDataForInterview(Guid interviewId);

        void MoveInterviewsBinaryDataToSyncFolder(Guid interviewId);
        IList<InterviewBinaryData> GetBinaryFilesFromSyncFolder();
        void RemoveBinaryDataFromSyncFolder(Guid interviewId, string fileName);
    }
}
