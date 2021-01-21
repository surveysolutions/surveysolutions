using System;
using Microsoft.Extensions.Options;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Shared.Web.Services;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public class VirtualPathService : IVirtualPathService
    {
        private readonly IOptions<HeadquartersConfig> options;
        private readonly IWorkspaceContextAccessor workspaceAccessor;

        public VirtualPathService(IOptions<HeadquartersConfig> options, IWorkspaceContextAccessor workspaceAccessor)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.workspaceAccessor = workspaceAccessor;
        }

        public string GetAbsolutePath(string relativePath)
        {
            // base: http://hq/primary, baseApp: http://hq/

            // ~/Webinterview = http://hq/primary/WebInterview
            // /primary/WebInterview = http://hq/primary/WebInterview

            if (relativePath.StartsWith('~'))
            {
                return relativePath.Replace("~", this.options.Value.BaseUrl);
            }

            if (relativePath.StartsWith('/'))
            {
                return this.options.Value.BaseAppUrl + relativePath;
            }

            throw new ArgumentException("Cannot determine absolute path. Relative Url should start from ~/ or from /");
        }

        public string GetRelatedToRootPath(string relativePath)
        {
            var workspace = this.workspaceAccessor.CurrentWorkspace();

            if (workspace == null) return relativePath.Replace("~", "");

            var pathBase = (workspace?.PathBase ?? string.Empty).TrimEnd('/');
            return $"{pathBase}/{workspace.Name}/{relativePath.Replace("~/", "")}";
        }
    }
}
