using System;
using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Designer.Code.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowOnlyFromWhitelistIPAttribute : TypeFilterAttribute
    {
        public AllowOnlyFromWhitelistIPAttribute(bool onlyAllowedAddresses = false) : base(typeof(IpAddressFilter))
        {
            Arguments = new object[] { onlyAllowedAddresses };
        }
    }
}
