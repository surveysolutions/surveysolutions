using System;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IInterviewBrokenPackagesService
    {
        bool HasBrokenPackageByInterview(Guid interviewId);
    }
}