using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization
{
    public interface IBrokenSyncPackagesStorage
    {
        IEnumerable<string> GetListOfUnhandledPackages();

        string GetUnhandledPackagePath(string package);

        void StoreUnhandledPackage(string unhandledPackagePath, Guid? interviewId, Exception e);
    }
}
