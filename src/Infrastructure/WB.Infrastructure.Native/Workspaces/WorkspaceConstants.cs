namespace WB.Infrastructure.Native.Workspaces
{
    public static class WorkspaceConstants
    {
        public const string SchemaName = "workspaces";
        public const string QuartzJobKey = "__workspace__";
        public const string ClaimType = "Workspace";
        public const string DefaultWorkspaceName = "primary";
        public const string RevisionClaimType = "WorkspaceRevision";

        public static bool IsSpecialWorkspace(string name)
        {
            return name == WorkspaceNames.AdminWorkspaceName
                   || name == WorkspaceNames.UsersWorkspaceName;
        }
        public static class WorkspaceNames
        {
            public const string UsersWorkspaceName = "users";
            public const string AdminWorkspaceName = "administration";
        }
    }
}
