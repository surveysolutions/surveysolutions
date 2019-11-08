using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Shared.Web.Filters
{
    public class NoCacheAttribute : ResponseCacheAttribute
    {
        public NoCacheAttribute() : base()
        {
            Duration = 0;
            Location = ResponseCacheLocation.None;
            NoStore = true;
        }
    }
}
