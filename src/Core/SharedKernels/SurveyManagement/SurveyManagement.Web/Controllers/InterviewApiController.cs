using System.Linq;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviews;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize]
    public class InterviewApiController : BaseApiController
    {
        private readonly IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory;
        private readonly IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory;
        private readonly IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;

        public InterviewApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IViewFactory<AllInterviewsInputModel, AllInterviewsView> allInterviewsViewFactory,
            IViewFactory<TeamInterviewsInputModel, TeamInterviewsView> teamInterviewViewFactory,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory)
            : base(commandService, globalInfo, logger)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.teamInterviewViewFactory = teamInterviewViewFactory;
            this.changeStatusFactory = changeStatusFactory;
            this.interviewSummaryViewFactory = interviewSummaryViewFactory;
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
                input.SearchBy = data.Request.SearchBy;
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
                input.SearchBy = data.Request.SearchBy;
            }

            return this.teamInterviewViewFactory.Load(input);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Supervisor, Headquarter")]
        public InverviewChangeStateHistoryView ChangeStateHistory(ChangeStateHistoryViewModel data)
        {
            var interviewSummary =
                this.changeStatusFactory.Load(new ChangeStatusInputModel {InterviewId = data.InterviewId});

            if (interviewSummary == null)
                return null;

            return new InverviewChangeStateHistoryView()
            {
                HistoryItems = interviewSummary.StatusHistory.Select(x => new HistoryItemView()
                {
                    Comment = x.Comment,
                    Date = x.Date.ToShortDateString(),
                    State = x.Status.ToLocalizeString(),
                    Responsible = x.Responsible
                })
            };
        }

        [Authorize(Roles = "Administrator, Headquarter")]
        public InterviewSummaryForMapPointView InterviewSummaryForMapPoint(InterviewSummaryForMapPointViewModel data)
        {
            if (data == null)
                return null;

            var interviewSummaryView = this.interviewSummaryViewFactory.Load(data.InterviewId);
            if (interviewSummaryView == null)
                return null;

            var interviewSummaryForMapPointView = new InterviewSummaryForMapPointView()
            {
                InterviewerName = interviewSummaryView.ResponsibleName,
                SupervisorName = interviewSummaryView.TeamLeadName
            };

            interviewSummaryForMapPointView.LastStatus = interviewSummaryView.Status.ToLocalizeString();
            interviewSummaryForMapPointView.LastUpdatedDate = AnswerUtils.AnswerToString(interviewSummaryView.UpdateDate);

            return interviewSummaryForMapPointView;
        }
    }
}