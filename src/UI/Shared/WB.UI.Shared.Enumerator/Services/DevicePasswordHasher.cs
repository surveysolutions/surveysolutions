using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Shared.Enumerator.Services
{
    public class DevicePasswordHasher : IPasswordHasher
    {
        private const uint CurrentHashImplementationVersion = 1;

        public string Hash(string password)
        {
            var rng = new RNGCryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(password);
            var saltBuffer = new byte[20];
            rng.GetBytes(saltBuffer);
            var hash = SHA512.Create().ComputeHash(data.Concat(saltBuffer).ToArray());
            return CurrentHashImplementationVersion + ":" + Convert.ToBase64String(saltBuffer) + ":" + Convert.ToBase64String(hash);
        }

        public PasswordVerificationResult VerifyPassword(string hashedPassword, string password)
        {
            if (password == null) return PasswordVerificationResult.Failed;

            var split = hashedPassword.Split(':');
            if (split.Length == 2)
            {
                return IsPasswordCorrect(0, hashedPassword, password)
                    ? PasswordVerificationResult.SuccessRehashNeeded
                    : PasswordVerificationResult.Failed;
            }

            if (split.Length != 3)
            {
                return PasswordVerificationResult.Failed;
            }

            if(!uint.TryParse(split[0], out var hashVersion))
            {
                return PasswordVerificationResult.Failed;
            }

            var isPasswordCorrect = IsPasswordCorrect(hashVersion, hashedPassword, password);

            if (isPasswordCorrect)
            {
                return CurrentHashImplementationVersion == hashVersion 
                    ? PasswordVerificationResult.Success 
                    : PasswordVerificationResult.SuccessRehashNeeded;
            }

            return PasswordVerificationResult.Failed;
        }

        private bool IsPasswordCorrect(uint hashVersion, string hashedPassword, string password)
        {
            switch (hashVersion)
            {
                case 0:
                    return PreKP11662Verification(hashedPassword, password);
                case 1:
                    return ConcatVerification(hashedPassword, password);
                default:
                    return false;
            }
        }

        private bool ConcatVerification(string hash, string password)
        {
            var split = hash.Split(':');
            var data = Encoding.UTF8.GetBytes(password);
            var newHash = SHA512.Create().ComputeHash(data.Concat(Convert.FromBase64String(split[1])).ToArray());
            var oldHash = Convert.FromBase64String(split[2]);
            return ByteArraysEqual(newHash, oldHash);
        }

        private bool PreKP11662Verification(string hash, string password)
        {
            var split = hash.Split(':');
            var data = Encoding.UTF8.GetBytes(password);
            var newHash = SHA512.Create().ComputeHash(data.Union(Convert.FromBase64String(split[0])).ToArray());
            var oldHash = Convert.FromBase64String(split[1]);
            return ByteArraysEqual(newHash, oldHash);
        }

        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }
            var areSame = true;
            for (var i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }
    }
}
