using System;
using System.IO;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace WB.UI.Headquarters.HealthChecks
{
    public class AspTempDirectoryAccessCheck : IHealthCheck
    {
        private const string AspTempEnvironmentVariable = "ASPNETCORE_TEMP";

        private readonly ILogger<AspTempDirectoryAccessCheck> logger;

        public AspTempDirectoryAccessCheck(ILogger<AspTempDirectoryAccessCheck> logger)
        {
            this.logger = logger;
        }
        
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            /*
               https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httprequestrewindextensions.enablebuffering?view=aspnetcore-6.0
               Remarks
                Temporary files for larger requests are written to the location named in the ASPNETCORE_TEMP 
                environment variable, if any. If that environment variable is not defined, these files are 
                written to the current user's temporary folder. Files are automatically deleted at the end of 
                their associated requests.
             */
            
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return Task.FromResult(HealthCheckResult.Healthy("System is not Windows"));
            
            var aspTempFolder = Environment.GetEnvironmentVariable(AspTempEnvironmentVariable);
            if (aspTempFolder == null)
            {
                // get current user's temporary folder
                aspTempFolder = Path.GetTempPath();
            }

            try
            {
                var hasWritePermission = CheckDirectoryPermissions(aspTempFolder);
                return Task.FromResult<HealthCheckResult>(
                    hasWritePermission
                        ? HealthCheckResult.Healthy() 
                        : HealthCheckResult.Unhealthy($"System can't write temp files to: {aspTempFolder}") );
            }
            catch (PrivilegeNotHeldException e)
            {
                return Task.FromResult(HealthCheckResult.Healthy($"Process hasn't privilege to check permissions of temp folder {aspTempFolder}"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't check temp folder permissions {aspTempFolder}", aspTempFolder);
                return Task.FromResult(HealthCheckResult.Degraded($"Can't check temp folder permissions {aspTempFolder}. Error: {ex.Message}"));
            }
        }

        private bool CheckDirectoryPermissions(string path)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new ArgumentException("This check supports only windows");
            
            DirectoryInfo di = new DirectoryInfo(path);
            DirectorySecurity acl = di.GetAccessControl(AccessControlSections.All);
            AuthorizationRuleCollection rules = acl.GetAccessRules(true, true, typeof(NTAccount));

            var accountName = Environment.UserName;

            foreach (AuthorizationRule rule in rules)
            {
                if (rule.IdentityReference.Value.Equals(accountName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var filesystemAccessRule = (FileSystemAccessRule)rule;

                    if ((filesystemAccessRule.FileSystemRights & FileSystemRights.WriteData) > 0 &&
                        filesystemAccessRule.AccessControlType != AccessControlType.Deny)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}