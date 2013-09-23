using System.Web.Http;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.Interviews;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using Web.Supervisor.Models;

namespace Web.Supervisor.Controllers
{
    public class InterviewApiController : BaseApiController
    {
        private readonly IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory;
        private readonly IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory;

        public InterviewApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
                                      IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory,
                                      IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.teamInterviewViewFactory = teamInterviewViewFactory;
        }

        [HttpPost]
        public AllInterviewsView AllInterviews(DocumentListViewModel data)
        {
            var input = new AllInterviewsInputModel
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
                input.QuestionnaireId = data.Request.TemplateId;
                input.QuestionnaireVersion = data.Request.TemplateVersion;
                input.TeamLeadId = data.Request.ResponsibleId;
                input.Status = data.Request.Status;
            }

            return this.allInterviewsViewFactory.Load(input);
        }

        [HttpPost]
        public TeamInterviewsView TeamInterviews(DocumentListViewModel data)
        {
            var input = new TeamInterviewsInputModel(viewerId: this.GlobalInfo.GetCurrentUser().Id)
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
                input.QuestionnaireId = data.Request.TemplateId;
                input.QuestionnaireVersion = data.Request.TemplateVersion;
                input.ResponsibleId = data.Request.ResponsibleId;
                input.Status = data.Request.Status;
            }

            return this.teamInterviewViewFactory.Load(input);
        }
    }
}