using Core.Supervisor.Views;
using Core.Supervisor.Views.Survey;

namespace Web.Supervisor.Controllers
{
    using System.Collections.Generic;
    using System.Web.Http;

    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Helpers;

    using WB.Core.SharedKernel.Logger;

    using Web.Supervisor.Models;

    [Authorize(Roles = "Headquarter, Supervisor")]
    public class SurveysApiController : BaseApiController
    {
        private readonly IViewFactory<SurveysInputModel, SurveysView> surveysViewFactory;
        private readonly IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory;

        public SurveysApiController(
            ICommandService commandService,
            IGlobalInfoProvider provider,
            ILog logger,
            IViewFactory<SurveysInputModel, SurveysView> surveysViewFactory, IViewFactory<SurveyUsersViewInputModel, SurveyUsersView> surveyUsersViewFactory)
            : base(commandService, provider, logger)
        {
            this.surveysViewFactory = surveysViewFactory;
            this.surveyUsersViewFactory = surveyUsersViewFactory;
        }

        public SurveysView Surveys(SurveyListViewModel data)
        {
            var input = new SurveysInputModel(
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
                input.UserId = data.Request.UserId;
            }

            return this.surveysViewFactory.Load(input);
        }

        public IEnumerable<SurveyUsersViewItem> Users()
        {
            return this.surveyUsersViewFactory.Load(new SurveyUsersViewInputModel(this.GlobalInfo.GetCurrentUser().Id,
                this.GlobalInfo.IsHeadquarter ? ViewerStatus.Headquarter : ViewerStatus.Supervisor)).Items;
        }
    }
}
