using Microsoft.AspNet.Identity;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    public class AspNetPasswordHasher : IPasswordHasher
    {
        private readonly Core.GenericSubdomains.Portable.IPasswordHasher passwordHasher;

        public AspNetPasswordHasher(Core.GenericSubdomains.Portable.IPasswordHasher passwordHasher)
        {
            this.passwordHasher = passwordHasher;
        }

        public string HashPassword(string password) => this.passwordHasher.Hash(password);

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
            => hashedPassword == this.passwordHasher.Hash(providedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
    }
}