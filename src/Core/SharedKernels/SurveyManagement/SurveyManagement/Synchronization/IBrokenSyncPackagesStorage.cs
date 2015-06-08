using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization
{
    public interface IBrokenSyncPackagesStorage
    {
        IEnumerable<string> GetListOfUnhandledPackages();

        string GetUnhandledPackagePath(string package);

        void StoreUnknownUnhandledPackage(string unhandledPackagePath, Exception e);

        void StoreUnhandledPackageForInterview(string unhandledPackagePath, Guid interviewId, Exception e);

        void StoreUnhandledPackageForInterviewInTypedFolder(string unhandledPackagePath, Guid interviewId, Exception e, string typeFolderName);
    }
}
