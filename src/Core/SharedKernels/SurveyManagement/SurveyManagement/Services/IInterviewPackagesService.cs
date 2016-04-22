using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IInterviewPackagesService
    {
        [Obsolete("Since v 5.7")]
        void StorePackage(string item);
        void StorePackage(InterviewPackage interview);
        int QueueLength { get; }
        int InvalidPackagesCount { get; }
        IReadOnlyCollection<string> GetTopPackageIds(int count);
        void ProcessPackage(string packageId);
        void ProcessPackage(InterviewPackage interview);
        bool HasPendingPackageByInterview(Guid interviewId);
        void ReprocessAllBrokenPackages();
    }
}
