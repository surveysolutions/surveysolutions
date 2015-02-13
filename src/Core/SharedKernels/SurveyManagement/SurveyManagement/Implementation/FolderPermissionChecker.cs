using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation
{
    public class FolderPermissionChecker : IFolderPermissionChecker
    {
        private readonly string folderPath;

        public FolderPermissionChecker(string folderPath)
        {
            this.folderPath = folderPath;
        }

        public FolderPermissionCheckResult Check()
        {
            HashSet<string> allowedFolders = new HashSet<string>();
            HashSet<string> deniedFolders = new HashSet<string>();

            var directories = GetAllSubFolders(folderPath, deniedFolders);

            var windowsIdentity = WindowsIdentity.GetCurrent();

            foreach (var directory in directories)
            {
                var writeAccess = CheckWriteAccess(windowsIdentity, directory);

                if (writeAccess)
                {
                    allowedFolders.Add(directory);
                }
                else
                {
                    deniedFolders.Add(directory);
                }
            }

            FolderPermissionCheckResult result = new FolderPermissionCheckResult(
                processRunedUnder: windowsIdentity.Name,
                allowedFolders: allowedFolders.ToArray(),
                denidedFolders: deniedFolders.ToArray());

            return result;
        }

        private IEnumerable<string> GetAllSubFolders(string path, HashSet<string> deniedFolders)
        {
            HashSet<string> folders = new HashSet<string>();
            folders.Add(path);

            try
            {
                var directories = Directory.GetDirectories(path);
                folders.UnionWith(directories);

                foreach (var directory in directories)
                {
                    var subFolders = GetAllSubFolders(directory, deniedFolders);
                    folders.UnionWith(subFolders);
                }
            }
            catch (UnauthorizedAccessException)
            {
                deniedFolders.Add(path);
                return Enumerable.Empty<string>();
            }

            return folders;
        }

        public bool CheckWriteAccess(WindowsIdentity currentUserReference, string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) 
                return false;

            try
            {
                DirectorySecurity security = Directory.GetAccessControl(path);
                var authorizationRuleCollection = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                var identityReferences = new List<IdentityReference> { currentUserReference.User };
                if (currentUserReference.Groups != null)
                    identityReferences.AddRange(currentUserReference.Groups);

                var isAllowWriteForUser = IsAllowWriteForIdentityReferance(authorizationRuleCollection, identityReferences);
                return isAllowWriteForUser;
            }
            catch(UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static bool IsAllowWriteForIdentityReferance(
            AuthorizationRuleCollection authorizationRuleCollection, List<IdentityReference> identityReferences)
        {
            var writeAllow = false;
            var writeDeny = false;

            foreach (AuthorizationRule authorizationRule in authorizationRuleCollection)
            {
                var isUserPermission = identityReferences.Any(ir => ir.Equals(authorizationRule.IdentityReference));
                if (isUserPermission)
                {
                    FileSystemAccessRule rule = ((FileSystemAccessRule) authorizationRule);

                    if ((rule.FileSystemRights & FileSystemRights.WriteData) == FileSystemRights.WriteData)
                    {
                        if (rule.AccessControlType == AccessControlType.Allow)
                            writeAllow = true;
                        else if (rule.AccessControlType == AccessControlType.Deny)
                            writeDeny = true;
                    }
                }
            }

            return writeAllow && !writeDeny;
        }
    }
}