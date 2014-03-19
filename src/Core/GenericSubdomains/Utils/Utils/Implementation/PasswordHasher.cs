using System;
using System.Text;
using WB.Core.GenericSubdomains.Utils.Implementation.Crypto;

namespace WB.Core.GenericSubdomains.Utils.Implementation
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
    }
}