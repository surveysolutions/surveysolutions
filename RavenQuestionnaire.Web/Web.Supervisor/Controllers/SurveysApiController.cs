using Core.Supervisor.Views;
using Core.Supervisor.Views.Reposts.InputModels;
using Core.Supervisor.Views.Reposts.Views;
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
        private readonly IViewFactory<HeadquarterSurveysAndStatusesReportInputModel, HeadquarterSurveysAndStatusesReportView> headquarterSurveysAndStatusesReport;

        public SurveysApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILogger logger,
            IViewFactory<HeadquarterSurveysAndStatusesReportInputModel, HeadquarterSurveysAndStatusesReportView> headquarterSurveysAndStatusesReport)
            : base(commandService, provider, logger)
        {
            this.headquarterSurveysAndStatusesReport = headquarterSurveysAndStatusesReport;
        }

        [HttpPost]
        public HeadquarterSurveysAndStatusesReportView HeadquarterSurveysAndStatusesReport(SurveyListViewModel data)
        {
            var input = new HeadquarterSurveysAndStatusesReportInputModel(this.GlobalInfo.GetCurrentUser().Id);

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

            return this.headquarterSurveysAndStatusesReport.Load(input);
        }
    }
}
