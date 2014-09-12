using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IPlainFileRepository
    {
        byte[] GetInterviewBinaryData(Guid interviewId, string fileName);
        string[] GetAllIdsOfBinaryDataByInterview(Guid interviewId);
        void StoreInterviewBinaryData(Guid interviewId, string fileName, byte[] data);
        void RemoveInterviewBinaryData(Guid interviewId, string fileName);
        void RemoveAllBinaryDataForInterview(Guid interviewId);
    }
}
