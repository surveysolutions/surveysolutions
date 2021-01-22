using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces
{
    public interface IWorkspacesUsersCache
    {
        void Invalidate(Guid userId);
        Task<List<string>> GetUserWorkspaces(Guid userId, CancellationToken token = default);
    }
}
