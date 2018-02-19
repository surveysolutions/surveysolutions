using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IInterviewPackagesService
    {
        [Obsolete("Since v 5.7")]
        void StoreOrProcessPackage(string item);
        void StoreOrProcessPackage(InterviewPackage interview);
        int QueueLength { get; }
        int InvalidPackagesCount { get; }
        IReadOnlyCollection<string> GetTopPackageIds(int count);
        void ProcessPackage(string packageId);
        void ProcessPackage(InterviewPackage interview);
        bool HasPendingPackageByInterview(Guid interviewId);
        void ReprocessSelectedBrokenPackages(int[] packageIds);

        IReadOnlyCollection<string> GetAllPackagesInterviewIds();
    }
}
