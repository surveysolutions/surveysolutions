using System.Linq;

namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class FolderPermissionCheckResult
    {
        public FolderPermissionCheckResult(HealthCheckStatus status, 
            string processRunedUnder, string[] allowedFolders, string[] deniedFolders)
        {
            ProcessRunedUnder = processRunedUnder;
            AllowedFolders = allowedFolders;
            DeniedFolders = deniedFolders;
            Status = status;
        }

        public string[] AllowedFolders { get; private set; }
        public string[] DeniedFolders { get; private set; }
        public string ProcessRunedUnder { get; private set; }
        public HealthCheckStatus Status { get; private set; }
    }
}