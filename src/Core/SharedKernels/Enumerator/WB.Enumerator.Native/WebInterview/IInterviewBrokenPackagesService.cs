using System;
using System.Collections.Generic;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IInterviewBrokenPackagesService
    {
        int InvalidPackagesCount { get; }

        bool IsNeedShowBrokenPackageNotificationForInterview(Guid interviewId);

        IReadOnlyCollection<int> GetTopBrokenPackageIdsAllowedToReprocess(int count);

        void ReprocessSelectedBrokenPackages(int[] packageIds);
    }
}
