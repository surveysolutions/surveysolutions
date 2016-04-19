using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IInterviewPackagesService
    {
        [Obsolete("Since v 5.7")]
        void StorePackage(string item);
        void StorePackage(Guid interviewId, Guid questionnaireId, long questionnaireVersion, Guid responsibleId,
            InterviewStatus interviewStatus, bool isCensusInterview, string events);
        int QueueLength { get; }
        int InvalidPackagesCount { get; }
        IReadOnlyCollection<string> GetTopPackageIds(int count);
        void ProcessPackage(string packageId);
        bool HasPendingPackageByInterview(Guid interviewId);
        void ReprocessAllBrokenPackages();
    }
}
