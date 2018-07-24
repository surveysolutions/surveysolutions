using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Shared.Enumerator.Services
{
    public class DevicePasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            var rng = new RNGCryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(password);
            var saltBuffer = new byte[20];
            rng.GetBytes(saltBuffer);
            var hash = SHA512.Create().ComputeHash(data.Concat(saltBuffer).ToArray());
            return Convert.ToBase64String(saltBuffer) + ":" + Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            if (password == null) return false;

            var split = hashedPassword.Split(':');
            if (split.Length != 2) return false;
            var data = Encoding.UTF8.GetBytes(password);

            var newHash = SHA512.Create().ComputeHash(data.Concat(Convert.FromBase64String(split[0])).ToArray());
            
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
