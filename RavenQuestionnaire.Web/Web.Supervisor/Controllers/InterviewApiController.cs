using System.Web.Http;
using Core.Supervisor.Views.ChangeStatus;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.Interviews;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using Web.Supervisor.Models;
using System.Linq;

namespace Web.Supervisor.Controllers
{
    [Authorize]
    public class InterviewApiController : BaseApiController
    {
        private readonly IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory;
        private readonly IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory;
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory;

        public InterviewApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
                                      IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory,
                                      IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory,
                                      IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
                                      IViewFactory<InterviewDetailsInputModel, InterviewDetailsView> interviewDetailsFactory)
            : base(commandService, globalInfo, logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.teamInterviewViewFactory = teamInterviewViewFactory;
            this.changeStatusFactory = changeStatusFactory;
            this.interviewDetailsFactory = interviewDetailsFactory;
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

        [HttpPost]
        [Authorize(Roles = "Supervisor, Headquarter")]
        public InverviewChangeStateHistoryView ChangeStateHistory(ChangeStateHistoryViewModel data)
        {
            return new InverviewChangeStateHistoryView()
            {
                HistoryItems =
                    this.changeStatusFactory.Load(new ChangeStatusInputModel { InterviewId = data.InterviewId })
                        .StatusHistory.Select(x => new HistoryItemView()
                        {
                            Comment = x.Comment,
                            Date = x.Date.ToShortDateString(),
                            State = x.Status.ToLocalizeString()
                        })
            };
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor, Headquarter")]
        public InterviewDetailsView InterviewDetails(InterviewDetailsViewModel data)
        {
            return this.interviewDetailsFactory.Load(
                 new InterviewDetailsInputModel()
                 {
                     CompleteQuestionnaireId = data.InterviewId,
                     CurrentGroupPublicKey = data.CurrentGroupId,
                     PropagationKey = data.CurrentPropagationKey,
                     User = this.GlobalInfo.GetCurrentUser()
                 });
        }
    }
}