using System.Collections.Generic;
using NHibernate.Criterion;

namespace WB.Infrastructure.Native.Workspaces
{
    public static class WorkspaceConstants
    {
        public const string SchemaName = "workspaces";
        public const string QuartzJobKey = "__workspace__";
        public const string ClaimType = "Workspace";
        public const string DefaultWorkspaceName = "primary";
        public const string RevisionClaimType = "WorkspaceRevision";

        public static bool IsSystemDefinedWorkspace(string name)
        {
            return name == WorkspaceNames.AdminWorkspaceName
                   || name == WorkspaceNames.UsersWorkspaceName;
        }
        
        public static class WorkspaceNames
        {
            public static readonly string UsersWorkspaceName = "users";
            public static readonly string AdminWorkspaceName = "administration";
        }

        public static readonly HashSet<string> NotAllowedNames = new HashSet<string>()
        {
            "api",
            "apidocs",
            "graphql",
        };
        
        public static bool IsNotAllowedName(string name)
        {
            var lowerCaseName = name.Lower();
            return IsSystemDefinedWorkspace(name) || NotAllowedNames.Contains(lowerCaseName);
        }   
    }
}
