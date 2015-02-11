using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization
{
    public interface IBrokenSyncPackagesStorage
    {
        string[] GetListOfUnhandledPackagesForInterview(Guid interviewId);

        IEnumerable<string> GetListOfUnhandledPackages();

        string GetUnhandledPackagePath(string id);

        void StoreUnhandledPackage(string unhandledPackagePath, Guid? interviewId, Exception e);
    }
}
