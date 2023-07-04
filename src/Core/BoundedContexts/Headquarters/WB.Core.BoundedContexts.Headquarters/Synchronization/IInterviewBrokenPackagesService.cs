using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Enumerator.Native.WebInterview
{
    public interface IInterviewBrokenPackagesService
    {
        int InvalidPackagesCount { get; }

        bool IsNeedShowBrokenPackageNotificationForInterview(Guid interviewId);

        bool HasBrokenPackageWithUnknownType(Guid interviewId);

        IReadOnlyCollection<int> GetTopBrokenPackageIdsAllowedToReprocess(int count);

        void ReprocessSelectedBrokenPackages(int[] packageIds);

        void PutReason(int[] packageIds, InterviewDomainExceptionType requestErrorType);
    }
}
