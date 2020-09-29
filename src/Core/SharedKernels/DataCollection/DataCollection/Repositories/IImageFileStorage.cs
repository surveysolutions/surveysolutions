using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IImageFileStorage : IInterviewFileStorage
    {
        string GetPath(Guid interviewId, string filename = null);

        Task RemoveAllBinaryDataForInterviewsAsync(List<Guid> interviewIds);
    }
}
