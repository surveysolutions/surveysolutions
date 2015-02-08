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
            //string ntAccountName = Environment.UserDomainName;
            //SecurityIdentifier users = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            //SecurityIdentifier users = new SecurityIdentifier(WellKnownSidType.ApplicationPoolIdentity, null);
            NTAccount iisNtAccount = new NTAccount(@"IIS AppPool\DefaultAppPool");
            SecurityIdentifier securityIdentifier = (SecurityIdentifier)iisNtAccount.Translate(typeof(SecurityIdentifier));
            var windowsIdentity = WindowsIdentity.GetCurrent();
            var identityReference = windowsIdentity.User.Translate(typeof(NTAccount));
//            var securityIdentifier = (SecurityIdentifier) identityReference;
            string currentUserName = securityIdentifier.Value;

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

            FolderPermissionCheckResult result = new FolderPermissionCheckResult()
            {
                AllowedFolders = allowedFolders.ToArray(),
                DenidedFolders = deniedFolders.ToArray(),
                ProcessRunedUnder = windowsIdentity.Name
            };

            return result;
        }

        private IEnumerable<string> GetAllSubFolders(string path, HashSet<string> deniedFolders)
        {
            HashSet<string> folders = new HashSet<string>();

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

                foreach (AuthorizationRule rule in authorizationRuleCollection)
                {
                    if (currentUserReference.User.Equals(rule.IdentityReference))
                    {
                        FileSystemAccessRule rights = ((FileSystemAccessRule)rule);

                        if (rights.AccessControlType == AccessControlType.Allow)
                        {
                            if ((rights.FileSystemRights & FileSystemRights.WriteData) > 0)
                                return true;
                        }
                    }
                }

                foreach (var userGroup in currentUserReference.Groups)
                {
                    foreach (AuthorizationRule rule in authorizationRuleCollection)
                    {
                        if (userGroup.Equals(rule.IdentityReference))
                        {
                            FileSystemAccessRule rights = ((FileSystemAccessRule)rule);

                            if (rights.AccessControlType == AccessControlType.Allow)
                            {
                                if ((rights.FileSystemRights & FileSystemRights.WriteData) > 0)
                                    return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool CheckWriteAccess2(WindowsIdentity currentUserReference, string path)
        {
            var writeAllow = false;
            var writeDeny = false;

            DirectorySecurity accessControlList;
            try
            {
                accessControlList = Directory.GetAccessControl(path);
                if (accessControlList == null)
                    return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            var accessRules = accessControlList.GetAccessRules(true, true, typeof(SecurityIdentifier));

            foreach (FileSystemAccessRule rule in accessRules)
            {
                if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                    continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    writeAllow = true;
                else if (rule.AccessControlType == AccessControlType.Deny)
                    writeDeny = true;
            }

            return writeAllow && !writeDeny;
        }

        public bool CheckWriteAccess3(WindowsIdentity currentUserReference, string path)
        {
            var permissionSet = new PermissionSet(PermissionState.None);
            var writePermission = new FileIOPermission(FileIOPermissionAccess.Write, path);
            permissionSet.AddPermission(writePermission);

            if (permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
            {
                return true;
            }
            return false;
        }
    }
}