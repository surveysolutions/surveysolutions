using Core.Supervisor.Views;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
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

        public SummaryApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IViewFactory<SummaryInputModel, SummaryView> summaryViewFactory)
            : base(commandService, provider, logger)
        {
            this.summaryViewFactory = summaryViewFactory;
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
    }
}
