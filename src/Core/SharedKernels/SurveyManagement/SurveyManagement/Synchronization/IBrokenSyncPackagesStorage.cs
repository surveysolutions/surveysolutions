using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization
{
    [Obsolete("Since v 5.8")]
    public interface IBrokenSyncPackagesStorage
    {
        IEnumerable<string> GetListOfUnhandledPackages();

        string GetUnhandledPackagePath(string package);

        void StoreUnhandledPackage(string unhandledPackagePath, Guid? interviewId, Exception e);
    }
}
