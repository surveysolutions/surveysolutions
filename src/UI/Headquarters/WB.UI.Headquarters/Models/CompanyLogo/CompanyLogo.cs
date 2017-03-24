using System;
using System.Security.Cryptography;

namespace WB.UI.Headquarters.Models.CompanyLogo
{
    public class CompanyLogo
    {
        public byte[] Logo { get; set; }

        public static readonly string StorageKey = "company logo";

        public string GetEtagValue()
        {
            using (var hasher = SHA1.Create())
            {
                var computeHash = hasher.ComputeHash(this.Logo);
                string hash = BitConverter.ToString(computeHash).Replace("-", "");
                return hash;
            }
        }
    }
}