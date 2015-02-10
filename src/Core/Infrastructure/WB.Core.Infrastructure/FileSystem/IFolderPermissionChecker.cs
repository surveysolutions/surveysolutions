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
        public string[] AllowedFolders { get; set; }
        public string[] DenidedFolders { get; set; }
        public string ProcessRunedUnder { get; set; }

        public HealthCheckStatus Status
        {
            get { return DenidedFolders.Any() ? HealthCheckStatus.Down : HealthCheckStatus.Happy; }
        }
    }
}