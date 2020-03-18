using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.Versions;

namespace WB.Core.BoundedContexts.Headquarters.Users.Providers
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

        private string firstVersionCache = null;

        public bool IsInSha1CompatibilityMode()
        {
            var firstVersionString = firstVersionCache 
                ?? (firstVersionCache = this.productVersion.GetHistory().OrderBy(h => h.UpdateTimeUtc).FirstOrDefault()?.ProductVersion);

            if (string.IsNullOrWhiteSpace(firstVersionString)) return false;

            var firstVersion = new Version(firstVersionString.Split(' ')[0]);
            return firstVersion < Compatible;
        }

        private static readonly Version Compatible = new Version(5, 19);

        public string GetSHA1HashFor(HqUser user, string password)
        {
            return this.oldPasswordHasher.Hash(password);
        }
    }
}