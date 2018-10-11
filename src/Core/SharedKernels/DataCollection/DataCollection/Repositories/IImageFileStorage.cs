using System;

namespace WB.Core.SharedKernels.DataCollection.Repositories
{
    public interface IImageFileStorage : IInterviewFileStorage
    {
        string GetPath(Guid interviewId, string filename = null);
    }
}
