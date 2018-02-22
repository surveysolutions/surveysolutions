using System;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IInterviewBrokenPackagesService
    {
        bool IsNeedShowBrokenPackageNotificationForInterview(Guid interviewId);
    }
}