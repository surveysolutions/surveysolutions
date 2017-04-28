using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.TabletInformation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Controllers
{
    public class TabletReportApiController : BaseApiController
    {
        private readonly ITabletInformationService tabletInformationService;

        public TabletReportApiController(
            ICommandService commandService,
            ILogger logger,
            ITabletInformationService tabletInformationService)
            : base(commandService, logger)
        {
            this.tabletInformationService = tabletInformationService;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public TabletInformationsView Packages(TabletInfoReportListViewModel input)
        {
            var items = this.tabletInformationService.GetAllTabletInformationPackages();

            if (!string.IsNullOrEmpty(input.SearchBy))
                items = items.Where(x => x.UserName != null && x.UserName.ToLower().StartsWith(input.SearchBy.ToLower())).ToList();

            var itemsSlice = items.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize);
                return new TabletInformationsView()
                {
                    Items = itemsSlice,
                    TotalCount = items.Count
                };
        }
    }
}