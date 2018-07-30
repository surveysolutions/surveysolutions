using System.Text;
using WB.Core.GenericSubdomains.Portable.Implementation.Crypto;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class PasswordHasher : IPasswordHasher
    {
        private static readonly string salt = "3858f62230ac3c915f300c664312c63f";

        public string Hash(string password)
        {
            var hash = new SHA1();
            
            HashResult result = hash.ComputeString(password + salt, Encoding.UTF8);

            return result.ToString().ToLower();
        }

        public PasswordVerificationResult VerifyPassword(string hashedPassword, string password)
        {
            return hashedPassword == Hash(password)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }
}
