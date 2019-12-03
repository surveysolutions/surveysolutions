using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
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

    public class HqUserStore : 
        UserStoreBase<HqUser, Guid, HqUserClaim, HqUserLogin, HqUserToken>,
        IUserRepository
    {
        private readonly IUnitOfWork unitOfWork;

        public HqUserStore(IUnitOfWork unitOfWork, IdentityErrorDescriber describer) : base (describer)
        {
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Microsoft.AspNetCore.Identity.IdentityResult> CreateAsync(HqUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            await unitOfWork.Session.SaveAsync(user, cancellationToken);
            await unitOfWork.Session.FlushAsync(cancellationToken);
            return Microsoft.AspNetCore.Identity.IdentityResult.Success;
        }

        public override async Task<Microsoft.AspNetCore.Identity.IdentityResult> UpdateAsync(HqUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            await this.unitOfWork.Session.UpdateAsync(user, cancellationToken);
            return Microsoft.AspNetCore.Identity.IdentityResult.Success;
        }

        public override async Task<Microsoft.AspNetCore.Identity.IdentityResult> DeleteAsync(HqUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            await this.unitOfWork.Session.DeleteAsync(user, cancellationToken);
            return Microsoft.AspNetCore.Identity.IdentityResult.Success;
        }

        public override Task<HqUser> FindByIdAsync(string userId, CancellationToken cancellationToken = new CancellationToken())
        {
            return this.unitOfWork.Session.GetAsync<HqUser>(Guid.Parse(userId), cancellationToken);
        }

        public Task<HqUser> FindByIdAsync(Guid userId, CancellationToken cancellationToken = new CancellationToken())
        {
            return this.FindByIdAsync(userId.ToString("N"), cancellationToken);
        }

        public override async Task<HqUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await unitOfWork.Session.QueryOver<HqUser>()
                .WhereRestrictionOn(x => x.UserName).IsInsensitiveLike(normalizedUserName)
                .SingleOrDefaultAsync<HqUser>(cancellationToken);
            return result;
        }

        protected override async Task<HqUser> FindUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await this.unitOfWork.Session.GetAsync<HqUser>(userId, cancellationToken).ConfigureAwait(false);
            return user;
        }

        protected override async Task<HqUserLogin> FindUserLoginAsync(Guid userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var user = await FindUserAsync(userId, cancellationToken).ConfigureAwait(false);
            return user.Logins.FirstOrDefault(x =>
                x.LoginProvider?.Equals(loginProvider, StringComparison.OrdinalIgnoreCase) == true &&
                x.ProviderKey?.Equals(providerKey) == true);
        }

        protected override Task<HqUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return Task.FromResult((HqUserLogin)null);
        }

        public override Task<IList<Claim>> GetClaimsAsync(HqUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(user.Claims.ToList() as IList<Claim>);
        }

        public override Task AddClaimsAsync(HqUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var claim in claims)
            {
                user.Claims.Add(new HqUserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    UserId = user.Id
                });
            }

            return Task.CompletedTask;
        }

        public override Task ReplaceClaimAsync(HqUser user, Claim claim, Claim newClaim,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override Task RemoveClaimsAsync(HqUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override Task<IList<HqUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        protected override Task<HqUserToken> FindTokenAsync(HqUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected override Task AddUserTokenAsync(HqUserToken token)
        {
            throw new NotImplementedException();
        }

        protected override Task RemoveUserTokenAsync(HqUserToken token)
        {
            throw new NotImplementedException();
        }

        public override Task AddLoginAsync(HqUser user, UserLoginInfo login, CancellationToken cancellationToken = new CancellationToken())
        {
            user.Logins.Add(new HqUserLogin
            {
                UserId = user.Id,
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName
            });

            return Task.CompletedTask;
        }

        public override Task RemoveLoginAsync(HqUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override Task<IList<UserLoginInfo>> GetLoginsAsync(HqUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override Task<HqUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = new CancellationToken())
        {
            return this.unitOfWork.Session.QueryOver<HqUser>().WhereRestrictionOn(x => x.Email)
                .IsInsensitiveLike(normalizedEmail).SingleOrDefaultAsync(cancellationToken);
        }

        public override IQueryable<HqUser> Users => unitOfWork.Session.Query<HqUser>();

        public Task<string> GetEmailAsync(HqUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.Email);
        }

        public async Task<HqUser> FindByEmailAsync(string email)
        {
            var result = await unitOfWork.Session.QueryOver<HqUser>()
                .WhereRestrictionOn(x => x.Email).IsInsensitiveLike(email)
                .SingleOrDefaultAsync<HqUser>();

            return result;
        }

        public IQueryable<DeviceSyncInfo> DeviceSyncInfos => unitOfWork.Session.Query<DeviceSyncInfo>();

        public HqRole FindRole(Guid id)
        {
            return this.unitOfWork.Session.Get<HqRole>(id);
        }

        public HqUser FindById(Guid userId)
        {
            return unitOfWork.Session.Get<HqUser>(userId);
        }
    }
}
