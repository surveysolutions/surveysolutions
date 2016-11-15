using System.Linq;
using System.Web.Http;
using NHibernate.Util;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Infrastructure.Native.Sanitizer;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize]
    public class InterviewApiController : BaseApiController
    {
        private readonly IAllInterviewsFactory allInterviewsViewFactory;
        private readonly ITeamInterviewsFactory teamInterviewViewFactory;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;

        public InterviewApiController(ICommandService commandService, IGlobalInfoProvider globalInfo, ILogger logger,
            IAllInterviewsFactory allInterviewsViewFactory,
            ITeamInterviewsFactory teamInterviewViewFactory,
            IChangeStatusFactory changeStatusFactory,
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
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Orders = data.SortOrder,

                QuestionnaireId = data.TemplateId,
                QuestionnaireVersion = data.TemplateVersion,
                TeamLeadName = data.ResponsibleName,
                Status = data.Status,
                SearchBy = data.SearchBy
            };

            var allInterviews = this.allInterviewsViewFactory.Load(input);

            allInterviews.Items.ForEach(x => x.FeaturedQuestions.ForEach(y => y.Question = y.Question.RemoveHtmlTags()));

            return allInterviews;
        }

        [HttpPost]
        public TeamInterviewsView TeamInterviews(DocumentListViewModel data)
        {
            var input = new TeamInterviewsInputModel
            {
                Page = data.PageIndex,
                PageSize = data.PageSize,
                Orders = data.SortOrder,
                QuestionnaireId = data.TemplateId,
                QuestionnaireVersion = data.TemplateVersion,
                SearchBy = data.SearchBy,
                Status = data.Status,
                ResponsibleName = data.ResponsibleName,
                ViewerId = this.GlobalInfo.GetCurrentUser().Id
            };

            var teamInterviews =  this.teamInterviewViewFactory.Load(input);

            teamInterviews.Items.ForEach(x => x.FeaturedQuestions.ForEach(y => y.Question = y.Question.RemoveHtmlTags()));

            return teamInterviews;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Supervisor, Headquarter")]
        public InverviewChangeStateHistoryView ChangeStateHistory(ChangeStateHistoryViewModel data)
        {
            var interviewSummary =
                this.changeStatusFactory.Load(new ChangeStatusInputModel {InterviewId = data.InterviewId});

            if (interviewSummary == null)
                return null;

            return new InverviewChangeStateHistoryView
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