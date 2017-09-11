using System;
using System.Security.Cryptography;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;

namespace WB.UI.Headquarters.Models.CompanyLogo
{
    public class CompanyLogo : AppSetting
    {
        public byte[] Logo { get; set; }

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