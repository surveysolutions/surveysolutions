using System;
using Microsoft.AspNetCore.Http;
using WB.UI.Shared.Web.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class VirtualPathService : IVirtualPathService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public VirtualPathService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetAbsolutePath(string relativePath)
        {
            return relativePath.TrimStart('~');
            /*var request = httpContextAccessor.HttpContext.Request;
            var url = new Uri(new Uri($"{request.Scheme}://{request.Host.Value}"), relativePath.TrimStart('~')).ToString();
            return url;*/
        }
    }
}
