using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Users
{
    public class HqRoleStore : IRoleStore<HqRole>
    {
        private readonly IUnitOfWork uow;

        public HqRoleStore(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        public void Dispose()
        {
        }

        public Task<Microsoft.AspNetCore.Identity.IdentityResult> CreateAsync(HqRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Microsoft.AspNetCore.Identity.IdentityResult> UpdateAsync(HqRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Microsoft.AspNetCore.Identity.IdentityResult> DeleteAsync(HqRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(HqRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString("N"));
        }

        public Task<string> GetRoleNameAsync(HqRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(HqRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedRoleNameAsync(HqRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name.ToUpper());
        }

        public Task SetNormalizedRoleNameAsync(HqRole role, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<HqRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return uow.Session.GetAsync<HqRole>(Guid.Parse(roleId), cancellationToken);
        }

        public Task<HqRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return uow.Session.QueryOver<HqRole>().WhereRestrictionOn(x => x.Name)
                .IsInsensitiveLike(normalizedRoleName).SingleOrDefaultAsync(cancellationToken);
        }
    }
}
