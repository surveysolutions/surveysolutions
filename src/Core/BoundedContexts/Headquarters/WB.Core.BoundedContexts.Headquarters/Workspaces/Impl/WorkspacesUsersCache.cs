using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Domain;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspacesUsersCache : IWorkspacesUsersCache
    {
        private readonly IInScopeExecutor<IUserRepository> usersRepository;

        // Global workspace independent cache
        private readonly IMemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        public WorkspacesUsersCache(IInScopeExecutor<IUserRepository> usersRepository)
        {
            this.usersRepository = usersRepository;
        }

        public void Invalidate(Guid userId)
        {
            this.memoryCache.Remove(userId.FormatGuid());
        }

        public async Task<List<string>> GetUserWorkspaces(Guid userId, CancellationToken token = default)
        {
            return await this.memoryCache.GetOrCreateAsync(userId.FormatGuid(), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                return await this.usersRepository.ExecuteAsync(async u =>
                {
                    var user = await u.FindByIdAsync(userId, token);
                    return user.Workspaces.Select(w => w.Workspace.Name).OrderBy(o => o).ToList();
                });
            });
        }
    }
}
