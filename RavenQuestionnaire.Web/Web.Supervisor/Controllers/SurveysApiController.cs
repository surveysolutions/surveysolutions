using Core.Supervisor.Views;
using Core.Supervisor.Views.Survey;
using WB.Core.GenericSubdomains.Logging;

namespace Web.Supervisor.Controllers
{
    using System.Web.Http;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;
    using Web.Supervisor.Models;

    [Authorize(Roles = "Headquarter, Supervisor")]
    public class SurveysApiController : BaseApiController
    {
        private readonly IViewFactory<SurveysInputModel, SurveysView> surveysViewFactory;

        public SurveysApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IViewFactory<SurveysInputModel, SurveysView> surveysViewFactory)
            : base(commandService, provider, logger)
        {
            this.surveysViewFactory = surveysViewFactory;
        }

        public SurveysView Surveys(SurveyListViewModel data)
        {
            var input = new SurveysInputModel(
                this.GlobalInfo.GetCurrentUser().Id,
                this.GlobalInfo.IsHeadquarter ? ViewerStatus.Headquarter : ViewerStatus.Supervisor);

            if (data != null)
            {
                input.Orders = data.SortOrder;
                if (data.Pager != null)
                {
                    input.Page = data.Pager.Page;
                    input.PageSize = data.Pager.PageSize;
                }

                if (data.Request != null)
                {
                    input.UserId = data.Request.UserId;
                }
            }

            return this.surveysViewFactory.Load(input);
        }
    }
}
