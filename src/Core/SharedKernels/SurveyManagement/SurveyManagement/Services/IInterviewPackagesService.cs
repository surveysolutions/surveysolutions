using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IInterviewPackagesService
    {
        void Enqueue(Guid interviewId, string item);
        int QueueLength { get; }
        IReadOnlyCollection<string> GetTopSyncItemsAsFileNames(int count);
        void ProcessPackage(string packageId);
        bool HasPackagesByInterviewId(Guid interviewId);
    }
}
