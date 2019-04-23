using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace WB.Core.BoundedContexts.Designer.MembershipProvider
{
    public class PasswordHasher : PasswordHasher<DesignerIdentityUser>
    {
        public override PasswordVerificationResult VerifyHashedPassword(DesignerIdentityUser user, string hashedPassword,
            string providedPassword)
        {
            if (user.PasswordSalt == null) return base.VerifyHashedPassword(user, hashedPassword, providedPassword);

            var saltAndPwd = String.Concat(providedPassword, user.PasswordSalt);
            var bytes = Encoding.Default.GetBytes(saltAndPwd);
            var sha1 = SHA1.Create();
            var computedHash = sha1.ComputeHash(bytes);
            var computedHashString = Convert.ToBase64String(computedHash);
            if (computedHashString == hashedPassword)
            {
                user.PasswordSalt = null;
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            return PasswordVerificationResult.Failed;
        }
    }
}
