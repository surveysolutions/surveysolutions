using System;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation
{
    public class TenantSettings : AppSetting
    {
        public string TenantPublicId { get; set; }
    }
}
