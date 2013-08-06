using System.Linq;
using Core.Supervisor.Views;
using Core.Supervisor.Views.Status;
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
    public class StatusApiController : BaseApiController
    {
        private readonly IViewFactory<StatusViewInputModel, StatusView> summaryViewFactory;

        public StatusApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IViewFactory<StatusViewInputModel, StatusView> summaryViewFactory)
            : base(commandService, provider, logger)
        {
            this.summaryViewFactory = summaryViewFactory;
        }

        public StatusView Status(StatusListViewModel data)
        {
            var input = new StatusViewInputModel(
                this.GlobalInfo.GetCurrentUser().Id,
                this.GlobalInfo.IsHeadquarter ? ViewerStatus.Headquarter : ViewerStatus.Supervisor)
                            {
                                Orders = data.SortOrder
                            };
            if (data.Pager != null)
            {
                input.Page = data.Pager.Page;
                input.PageSize = data.Pager.PageSize;
            }

            if (data.Request != null)
            {
                input.StatusId = data.Request.StatusId;
            }

            return this.summaryViewFactory.Load(input);
        }
    }
}
