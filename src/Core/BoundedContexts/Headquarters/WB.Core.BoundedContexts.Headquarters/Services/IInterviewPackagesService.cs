using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IInterviewPackagesService
    {
        void StoreOrProcessPackage(InterviewPackage interview);
        int QueueLength { get; }
        IReadOnlyCollection<string> GetTopPackageIds(int count);
        void ProcessPackage(string packageId);
        void ProcessPackage(InterviewPackage interview);
        bool HasPendingPackageByInterview(Guid interviewId);
        bool IsPackageDuplicated(EventStreamSignatureTag eventStreamSignatureTag);

        IReadOnlyCollection<string> GetAllPackagesInterviewIds();
    }
}
