using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.Infrastructure.Versions;
using WB.UI.Shared.Web.Attributes;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api
{
    [NoTransaction]
    [LocalOrDevelopmentAccessOnly]
    public class ControlPanelApiController : ApiController
    {
        public class VersionsInfo
        {
            public VersionsInfo(string product, Dictionary<DateTime, string> history)
            {
                this.Product = product;
                this.History = history;
            }

            public string Product { get; }
            public Dictionary<DateTime, string> History { get; }
        }

        private readonly IProductVersion productVersion;
        private readonly IProductVersionHistory productVersionHistory;

        public ControlPanelApiController(
            IProductVersion productVersion,
            IProductVersionHistory productVersionHistory)
        {
            this.productVersion = productVersion;
            this.productVersionHistory = productVersionHistory;
        }

        [NoTransaction]
        public VersionsInfo GetVersions()
        {
            return new VersionsInfo(
                this.productVersion.ToString(),
                this.productVersionHistory.GetHistory().ToDictionary(
                    change => change.UpdateTimeUtc,
                    change => change.ProductVersion));
        }
    }
}