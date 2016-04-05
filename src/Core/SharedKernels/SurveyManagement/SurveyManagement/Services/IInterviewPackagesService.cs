using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IInterviewPackagesService
    {
        [Obsolete("Since v 5.7")]
        void StorePackage(Guid interviewId, string item);
        void StorePackage(Guid interviewId, Guid questionnaireId, long questionnaireVersion, Guid responsibleId,
            InterviewStatus interviewStatus, bool isCensusInterview, string events);
        int QueueLength { get; }
        IReadOnlyCollection<string> GetTopPackageIds(int count);
        void ProcessPackage(string packageId);
        bool HasPackagesByInterviewId(Guid interviewId);
        void ReprocessAllBrokenPackages();
    }
}
