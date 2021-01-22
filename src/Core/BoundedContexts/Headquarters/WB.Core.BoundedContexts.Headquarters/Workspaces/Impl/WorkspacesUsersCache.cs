using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.Workspaces.Impl
{
    class WorkspacesUsersCache : IWorkspacesUsersCache
    {
        private readonly IUserRepository usersRepository;

        // Global workspace independent cache
        private readonly IMemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        public WorkspacesUsersCache(IUserRepository usersRepository)
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
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                var user = await this.usersRepository.FindByIdAsync(userId, token);
                return user.Workspaces.Select(w => w.Workspace.Name).OrderBy(o => o).ToList();
            });
        }
    }
}
