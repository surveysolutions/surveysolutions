using Core.Supervisor.Views;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;

    using Core.Supervisor.Views.Summary;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;

    [Authorize(Roles = "Headquarter, Supervisor")]
    public class SummaryApiController : BaseApiController
    {
        private readonly IViewFactory<SummaryInputModel, SummaryView> summaryViewFactory;
        private readonly IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory;

        public SummaryApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IViewFactory<SummaryInputModel, SummaryView> summaryViewFactory, IViewFactory<SummaryTemplatesInputModel, SummaryTemplatesView> summaryTemplatesViewFactory)
            : base(commandService, provider, logger)
        {
            this.summaryViewFactory = summaryViewFactory;
            this.summaryTemplatesViewFactory = summaryTemplatesViewFactory;
        }

        public SummaryView Summary(SummaryListViewModel data)
        {
            var input = new SummaryInputModel(
                this.GlobalInfo.GetCurrentUser().Id,
                this.GlobalInfo.IsHeadquarter ? ViewerStatus.Headquarter : ViewerStatus.Supervisor)
                            {
                                Orders
                                    =
                                    data
                                    .SortOrder
                            };
            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            if (data.Request != null)
            {
                input.TemplateId = data.Request.TemplateId;
            }

            return this.summaryViewFactory.Load(input);
        }

        public IEnumerable<SummaryTemplateViewItem> Templates()
        {
            return this.summaryTemplatesViewFactory.Load(new SummaryTemplatesInputModel(
                this.GlobalInfo.GetCurrentUser().Id,
                this.GlobalInfo.IsHeadquarter ? ViewerStatus.Headquarter : ViewerStatus.Supervisor)).Items;
        }
    }
}
