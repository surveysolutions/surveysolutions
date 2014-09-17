using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IFileSyncRepository
    {
        void MoveInterviewsBinaryDataToSyncFolder(Guid interviewId);
        IList<InterviewBinaryData> GetBinaryFilesFromSyncFolder();
        void RemoveBinaryDataFromSyncFolder(Guid interviewId, string fileName);
    }
}
