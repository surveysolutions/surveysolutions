using System;
using System.Security.Cryptography;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.UI.Headquarters.Models.CompanyLogo
{
    [AppSetting(fallbackReadFromPrimaryWorkspace: true)]
    public class CompanyLogo : AppSetting
    {
        public byte[] Logo { get; set; }

        public string GetEtagValue()
        {
            using var hasher = SHA1.Create();
            var computeHash = hasher.ComputeHash(this.Logo);
            string hash = BitConverter.ToString(computeHash).Replace("-", "");
            return hash;
        }
    }
}
