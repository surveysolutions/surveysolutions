using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces;

public class WorkspacesFilterResult
{
    public WorkspacesFilterResult(int offset, int limit, int totalCount, List<WorkspaceFilterResultItem> workspaces)
    {
        Offset = offset;
        Limit = limit;
        TotalCount = totalCount;
        Workspaces = workspaces;
    }

    public int Offset { get; }
    public int Limit { get; }
    public int TotalCount { get; }
    public List<WorkspaceFilterResultItem> Workspaces { get; }
}