using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public class TabletReportApiController : BaseApiController
    {
        private readonly ITabletInformationService tabletInformationService;
        private readonly IInterviewersViewFactory interviewersFactory;

        public TabletReportApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            ITabletInformationService tabletInformationService,
            IInterviewersViewFactory interviewersFactory)
            : base(commandService, provider, logger)
        {
            this.tabletInformationService = tabletInformationService;
            this.interviewersFactory = interviewersFactory;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public TabletInformationsView Packages(TabletInfoReportListViewModel input)
        {
            var items = !string.IsNullOrEmpty(input.SearchBy) 
                      ? this.tabletInformationService.GetAllTabletInformationPackages()
                            .Where(x => x.UserName != null && x.UserName.ToLower().StartsWith(input.SearchBy.ToLower())).ToList()
                      : this.tabletInformationService.GetAllTabletInformationPackages();

            var itemsSlice = items.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize);
                return new TabletInformationsView()
                {
                    Items = itemsSlice,
                    TotalCount = items.Count
                };
        }
    }
}