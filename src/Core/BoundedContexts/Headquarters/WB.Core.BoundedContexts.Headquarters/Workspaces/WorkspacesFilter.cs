namespace WB.Core.BoundedContexts.Headquarters.Workspaces;

public class WorkspacesFilter
{
    public string Query { get; set; }
    public bool IncludeDisabled { get; set; } = false;
    public int Offset { get; set; }
    public int Limit { get; set; } = 10;
    public string UserId { get; set; }
    public string SortOrder { get; set; }
}