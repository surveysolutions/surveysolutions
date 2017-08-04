using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate.Util;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize]
    public class InterviewApiController : BaseApiController
    {
        private readonly IAuthorizedUser authorizedUser;
        private readonly IAllInterviewsFactory allInterviewsViewFactory;
        private readonly ITeamInterviewsFactory teamInterviewViewFactory;
        private readonly IChangeStatusFactory changeStatusFactory;
        private readonly IInterviewSummaryViewFactory interviewSummaryViewFactory;

        public InterviewApiController(ICommandService commandService, IAuthorizedUser authorizedUser, ILogger logger,
            IAllInterviewsFactory allInterviewsViewFactory,
            ITeamInterviewsFactory teamInterviewViewFactory,
            IChangeStatusFactory changeStatusFactory,
            IInterviewSummaryViewFactory interviewSummaryViewFactory)
            : base(commandService, logger)
        {
            this.authorizedUser = authorizedUser;
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
                Statuses = data.Status.HasValue ? new [] {data.Status.Value} : null,
                SearchBy = data.SearchBy,
                AssignmentId = data.AssignmentId
            };

            var allInterviews = this.allInterviewsViewFactory.Load(input);

            allInterviews.Items.ForEach(x => x.FeaturedQuestions.ForEach(y => y.Question = y.Question.RemoveHtmlTags()));

            return allInterviews;
        }

        [HttpGet]
        [Authorize(Roles = "Interviewer")]
        [CamelCase]
        public InterviewsDataTableResponse GetInterviews([FromUri] InterviewsDataTableRequest request)
        {
            QuestionnaireIdentity questionnaireIdentity = null;
            if (!string.IsNullOrEmpty(request.QuestionnaireId))
            {
                QuestionnaireIdentity.TryParse(request.QuestionnaireId, out questionnaireIdentity);
            }

            var input = new AllInterviewsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                QuestionnaireId = questionnaireIdentity?.QuestionnaireId,
                QuestionnaireVersion = questionnaireIdentity?.Version,
                Statuses = request.Statuses,
                SearchBy = request.SearchBy ?? request.Search.Value,
                ResponsibleId = this.authorizedUser.Id,
                AssignmentId = request.AssignmentId
            };

            var allInterviews = this.allInterviewsViewFactory.Load(input);

            allInterviews.Items.ForEach(x => x.FeaturedQuestions.ForEach(y => y.Question = y.Question.RemoveHtmlTags()));

            var response = new InterviewsDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = allInterviews.TotalCount,
                RecordsFiltered = allInterviews.TotalCount,
                Data = allInterviews.Items
            };

            return response;
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
                ViewerId = this.authorizedUser.Id,
                AssignmentId = data.AssignmentId
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