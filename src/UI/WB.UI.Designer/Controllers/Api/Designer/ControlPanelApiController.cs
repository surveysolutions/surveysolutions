using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.Infrastructure.Versions;

namespace WB.UI.Designer.Api
{
    [Authorize(Roles = nameof(SimpleRoleEnum.Administrator))]
    public class ControlPanelApiController : Controller
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
