using System;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Services.Impl
{
    public class VirtualPathService : IVirtualPathService
    {
        private readonly IOptions<HeadquartersConfig> options;

        public VirtualPathService(IOptions<HeadquartersConfig> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string GetAbsolutePath(string relativePath) => relativePath.Replace("~", this.GetHostUrl());
        public string GetHostUrl() => this.options.Value.BaseUrl;
    }
}
