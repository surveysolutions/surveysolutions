using System;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Code.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowOnlyFromWhitelistIPAttribute : TypeFilterAttribute
    {
        public AllowOnlyFromWhitelistIPAttribute() : base(typeof(IpAddressFilter))
        {
        }
    }
}
