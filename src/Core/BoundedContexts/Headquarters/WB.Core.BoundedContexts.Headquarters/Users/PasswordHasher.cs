using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using PasswordVerificationResult = WB.Core.GenericSubdomains.Portable.PasswordVerificationResult;

namespace WB.Core.BoundedContexts.Headquarters.Users
{
    public interface IIdentityPasswordHasher : IPasswordHasher
    {

    }

    public class HqPasswordHasher : PasswordHasher<HqUser>, IIdentityPasswordHasher
    {
        public HqPasswordHasher() : base(
            new OptionsManager<PasswordHasherOptions>(
                new OptionsFactory<PasswordHasherOptions>(
                    new List<IConfigureOptions<PasswordHasherOptions>>
                    {
                        new ConfigureOptions<PasswordHasherOptions>(x =>
                            x.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2)
                    },
                    new List<IPostConfigureOptions<PasswordHasherOptions>>())))
        {

        }

        public string Hash(string password) => base.HashPassword(null, password);

        public PasswordVerificationResult VerifyPassword(string hashedPassword, string password)
        {
            switch (base.VerifyHashedPassword(null, hashedPassword, password))
            {
                case Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success:
                    return PasswordVerificationResult.Success;
                case Microsoft.AspNetCore.Identity.PasswordVerificationResult.SuccessRehashNeeded:
                    return PasswordVerificationResult.SuccessRehashNeeded;
                default:
                    return PasswordVerificationResult.Failed;
            }
        }
    }
}
