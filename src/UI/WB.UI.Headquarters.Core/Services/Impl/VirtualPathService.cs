using System;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Services.Impl
{
    public class VirtualPathService : IVirtualPathService
    {
        private readonly IOptions<HeadquarterOptions> options;

        public VirtualPathService(IOptions<HeadquarterOptions> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string GetAbsolutePath(string relativePath) => relativePath.Replace("~", options.Value.BaseUrl);
    }
}
