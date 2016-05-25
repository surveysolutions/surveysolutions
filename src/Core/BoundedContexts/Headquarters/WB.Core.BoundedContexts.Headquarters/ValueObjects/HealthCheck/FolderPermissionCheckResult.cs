namespace WB.Core.BoundedContexts.Headquarters.ValueObjects.HealthCheck
{
    public class FolderPermissionCheckResult
    {
        public FolderPermissionCheckResult(HealthCheckStatus status, 
            string processRunedUnder, string[] allowedFolders, string[] deniedFolders)
        {
            this.ProcessRunedUnder = processRunedUnder;
            this.AllowedFolders = allowedFolders;
            this.DeniedFolders = deniedFolders;
            this.Status = status;
        }

        public string[] AllowedFolders { get; private set; }
        public string[] DeniedFolders { get; private set; }
        public string ProcessRunedUnder { get; private set; }
        public HealthCheckStatus Status { get; private set; }
    }
}