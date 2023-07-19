using System;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces;

public class WorkspaceFilterResultItem
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public DateTime? DisabledAtUtc { get; set; }
}