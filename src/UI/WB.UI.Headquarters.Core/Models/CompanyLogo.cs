using System;
using System.Security.Cryptography;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.SharedKernels.DataCollection.Helpers;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.UI.Headquarters.Models.CompanyLogo
{
    [AppSetting(fallbackReadFromPrimaryWorkspace: true)]
    public class CompanyLogo : AppSetting
    {
        public byte[] Logo { get; set; }

        public string GetEtagValue()
        {
            return CheckSumHelper.GetSha1Cache(this.Logo);
        }
    }
}
