using System.Linq;

namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.HealthCheck
{
    public class FolderPermissionCheckResult
    {
        public FolderPermissionCheckResult(HealthCheckStatus status, 
            string processRunedUnder, string[] allowedFolders, string[] denidedFolders)
        {
            ProcessRunedUnder = processRunedUnder;
            AllowedFolders = allowedFolders;
            DenidedFolders = denidedFolders;
            Status = status;
        }

        public string[] AllowedFolders { get; private set; }
        public string[] DenidedFolders { get; private set; }
        public string ProcessRunedUnder { get; private set; }
        public HealthCheckStatus Status { get; private set; }
    }
}