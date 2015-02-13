using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.HealthCheck;

namespace WB.Core.Infrastructure.FileSystem
{
    public interface IFolderPermissionChecker
    {
        FolderPermissionCheckResult Check();
    }

    public class FolderPermissionCheckResult
    {
        public FolderPermissionCheckResult(string processRunedUnder, string[] allowedFolders, string[] denidedFolders)
        {
            ProcessRunedUnder = processRunedUnder;
            AllowedFolders = allowedFolders;
            DenidedFolders = denidedFolders;
        }

        public string[] AllowedFolders { get; private set; }
        public string[] DenidedFolders { get; private set; }
        public string ProcessRunedUnder { get; private set; }

        public HealthCheckStatus Status
        {
            get { return DenidedFolders != null && DenidedFolders.Any() ? HealthCheckStatus.Down : HealthCheckStatus.Happy; }
        }
    }
}