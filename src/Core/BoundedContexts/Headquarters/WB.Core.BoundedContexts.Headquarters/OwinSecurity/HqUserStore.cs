using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    internal class HqUserStore : IUserRepository
    {
        private readonly IUnitOfWork unitOfWork;

        public HqUserStore(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IQueryable<HqUser> Users => unitOfWork.Session.Query<HqUser>();

        public Task<IList<string>> GetRolesAsync(Guid userId)
        {
            var user = FindById(userId);

            IList<string> roleNames = user.Roles.Select(x => x.Name).ToList();

            return Task.FromResult(roleNames);
        }

        public async Task<IEnumerable<Claim>> GetClaimsAsync(Guid userId)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "UserId {0} not found.",
                    userId));
            }
            
            return user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
        }

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

        public Task<string> GetSecurityStampAsync(HqUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.SecurityStamp);
        }

        public async Task CreateAsync(HqUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            unitOfWork.Session.Save(user);
            await unitOfWork.Session.FlushAsync();
        }

        public async Task UpdateAsync(HqUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            await unitOfWork.Session.UpdateAsync(user);
        }

        public Task<HqUser> FindByIdAsync(Guid userId)
        {
            return this.unitOfWork.Session.GetAsync<HqUser>(userId);
        }

        public HqUser FindById(Guid userId)
        {
            return unitOfWork.Session.Get<HqUser>(userId);
        }

        public async Task<HqUser> FindByNameAsync(string userName)
        {
            var result = await unitOfWork.Session.Query<HqUser>()
                .Where(u => u.UserName.ToUpper() == userName.ToUpper())
                .SingleOrDefaultAsync();

            return result;
        }

        public Task SetPasswordHashAsync(HqUser user, string hash)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.PasswordHash = hash;
            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(HqUser user, string newSecurityStamp)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.SecurityStamp = newSecurityStamp;
            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(HqUser user, bool isLockoutEnabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.LockoutEnabled = isLockoutEnabled;
            return Task.FromResult(0);
        }
    }
}
