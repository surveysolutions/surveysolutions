using System;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ILastCompletionComments
    {
        void Store(Guid interviewId, string comment);
        string Get(Guid interviewId);
        void Remove(Guid interviewId);
    }
}
