using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl;

public class WorkspacesStorage : IWorkspacesStorage
{
    private readonly IPlainStorageAccessor<Workspace> workspaces;
    private readonly IAuthorizedUser authorizedUser;

    public WorkspacesStorage(IPlainStorageAccessor<Workspace> workspaces,
        IAuthorizedUser authorizedUser)
    {
        this.workspaces = workspaces;
        this.authorizedUser = authorizedUser;
    }
    
    public List<WorkspaceContext> GetEnabledWorkspaces()
    {
        return workspaces.Query(_ => _
            .Where(x => x.DisabledAtUtc == null && x.RemovedAtUtc == null)
            .Select(workspace => workspace.AsContext())
            .ToList());
    }

    public List<WorkspaceContext> GetAllWorkspaces()
    {
        return workspaces.Query(_ => _
            .Where(w => w.RemovedAtUtc == null)
            .Select(w => w.AsContext()
            ).ToList());
    }

    public void Store(Workspace workspace)
    {
        workspaces.Store(workspace, workspace.Name);
    }

    public void Delete(string workspaceName)
    {
        if (workspaceName == WorkspaceConstants.DefaultWorkspaceName)
        {
            return;
        }

        var w = this.workspaces.GetById(workspaceName);
        w.Remove();
        this.workspaces.Store(w, workspaceName);
    }

    public Workspace GetById(string workspace) => workspaces.GetById(workspace);
    public Task<Workspace> GetByIdAsync(string workspace) => workspaces.GetByIdAsync(workspace);
    
    public WorkspacesFilterResult FilterWorkspaces(WorkspacesFilter filter)
    {
        var result =
            this.workspaces.Query(_ =>
                    Filter(filter, _)
                        .Skip(filter.Offset)
                        .Take(filter.Limit)
                        .ToList())
                .Select(x => new WorkspaceFilterResultItem()
                {
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    DisabledAtUtc = x.DisabledAtUtc,
                    CreatedAtUtc = x.CreatedAtUtc
                }).ToList();
        int totalCount = this.workspaces.Query(_ => Filter(filter, _).Count());

        return new WorkspacesFilterResult
        (
            filter.Offset,
            filter.Limit,
            totalCount,
            result
        );
    }

    public bool HasWorkspaceWithName(string name)
    {
        return workspaces.Query(_ => _.Any(x => x.Name == name));
    }

    private IQueryable<Workspace> Filter(WorkspacesFilter filter, IQueryable<Workspace> source)
    {
        IQueryable<Workspace> result = source
            .Where(w => w.RemovedAtUtc == null)
            .OrderBy(x => x.DisplayName);

        if (!this.authorizedUser.IsAdministrator)
        {
            var userWorkspaces = this.authorizedUser.Workspaces.ToList();
            result = result.Where(x => userWorkspaces.Contains(x.Name));
        }

        if (!string.IsNullOrEmpty(filter.UserId))
        {
            result = result.Where(x => x.Users.Any(u => u.User.Id.ToString() == filter.UserId));
        }

        if (!string.IsNullOrEmpty(filter.Query))
        {
            var lowerCaseQuery = filter.Query.ToLower();
            result = result.Where(w => w.DisplayName.ToLower().Contains(lowerCaseQuery));
        }

        if (!filter.IncludeDisabled)
        {
            result = result.Where(w => w.DisabledAtUtc == null);
        }

        return result;
    }

}
