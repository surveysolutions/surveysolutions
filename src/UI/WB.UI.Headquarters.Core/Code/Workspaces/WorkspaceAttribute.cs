using System;

namespace WB.UI.Headquarters.Code.Workspaces
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WorkspaceAttribute : Attribute
    {
        public string WorkspaceName { get; }

        public WorkspaceAttribute(string workspaceName)
        {
            WorkspaceName = workspaceName;
        }
    }
}