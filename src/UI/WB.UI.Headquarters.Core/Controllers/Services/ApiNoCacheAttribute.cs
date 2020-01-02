using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Headquarters.Controllers.Services
{
    public class ApiNoCacheAttribute : ResponseCacheAttribute
    {
        public ApiNoCacheAttribute()
        {
            NoStore = true;
        }
    }
}