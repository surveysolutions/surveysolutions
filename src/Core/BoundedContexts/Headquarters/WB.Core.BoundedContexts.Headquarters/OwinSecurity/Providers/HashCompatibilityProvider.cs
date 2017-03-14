using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Versions;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity.Providers
{
    public class HashCompatibilityProvider : IHashCompatibilityProvider
    {
        private readonly IProductVersionHistory productVersion;
        private readonly GenericSubdomains.Portable.IPasswordHasher oldPasswordHasher;

        public HashCompatibilityProvider(IProductVersionHistory productVersion, 
            GenericSubdomains.Portable.IPasswordHasher oldPasswordHasher)
        {
            this.productVersion = productVersion;
            this.oldPasswordHasher = oldPasswordHasher;
        }

        public bool IsSHA1Required(HqUser user)
        {
            if (!user.IsInRole(UserRoles.Interviewer)) return false;

            var firstVersionString = this.productVersion.GetHistory().OrderBy(h => h.UpdateTimeUtc).FirstOrDefault().ProductVersion;

            if (string.IsNullOrWhiteSpace(firstVersionString)) return false;

            var firstVersion = new Version(firstVersionString.Split(' ')[0]);
            return firstVersion < Compatible;
        }
        
        private static readonly Version Compatible = new Version(5, 19);

        public string GetSHA1HashFor(HqUser user, string password)
        {
            if (this.IsSHA1Required(user))
            {
                return this.oldPasswordHasher.Hash(password);
            }

            return null;
        }
    }
}