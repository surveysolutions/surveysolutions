using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Attributes;

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

        [HttpGet]
        [CamelCase]
        public InterviewsDataTableResponse Interviews([FromUri] InterviewsDataTableRequest request)
        {

            var input = new AllInterviewsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                QuestionnaireId = request.QuestionnaireId,
                QuestionnaireVersion = request.QuestionnaireVersion,
                SupervisorOrInterviewerName = request.ResponsibleName,
                Statuses = request.Status != null ? new[] { request.Status.Value } : null,
                SearchBy = request.SearchBy ?? request.Search.Value,
                TeamId = request.TeamId,
                UnactiveDateStart = request.UnactiveDateStart?.ToUniversalTime(),
                UnactiveDateEnd = request.UnactiveDateEnd?.ToUniversalTime(),
                AssignmentId = request.AssignmentId,
                //ResponsibleId = request.ResponsibleId
            };

            var allInterviews = this.allInterviewsViewFactory.Load(input);

            foreach (var x in allInterviews.Items)
            {
                Enum.TryParse(x.Status, out InterviewStatus myStatus);
                x.Status = myStatus.ToLocalizeString();

                foreach (var y in x.FeaturedQuestions)
                    y.Question = y.Question.RemoveHtmlTags();
            }

            var response = new InterviewsDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = allInterviews.TotalCount,
                RecordsFiltered = allInterviews.TotalCount,
                Data = allInterviews.Items
            };

            return response;
        }

        [HttpGet]
        [Authorize(Roles = "Interviewer")]
        [CamelCase]
        public InterviewsDataTableResponse GetInterviews([FromUri] InterviewsDataTableRequest request)
        {
            
            var input = new AllInterviewsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                QuestionnaireId = request.QuestionnaireId,
                QuestionnaireVersion = request.QuestionnaireVersion,
                Statuses = request.Statuses,
                SearchBy = request.SearchBy ?? request.Search.Value,
                ResponsibleId = this.authorizedUser.Id,
                AssignmentId = request.AssignmentId
            };

            var allInterviews = this.allInterviewsViewFactory.Load(input);

            foreach (var x in allInterviews.Items) foreach (var y in x.FeaturedQuestions) y.Question = y.Question.RemoveHtmlTags();

            var response = new InterviewsDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = allInterviews.TotalCount,
                RecordsFiltered = allInterviews.TotalCount,
                Data = allInterviews.Items
            };

            return response;
        }
        
        [HttpGet]
        [CamelCase]
        public TeamInterviewsDataTableResponse GetTeamInterviews([FromUri] InterviewsDataTableRequest request)
        {
            var input = new TeamInterviewsInputModel
            {
                Page = request.PageIndex,
                PageSize = request.PageSize,
                Orders = request.GetSortOrderRequestItems(),
                QuestionnaireId = request.QuestionnaireId,
                QuestionnaireVersion = request.QuestionnaireVersion,
                SearchBy = request.SearchBy,
                Status = request.Status,
                ResponsibleName = request.ResponsibleName,
                ViewerId = this.authorizedUser.Id,
                UnactiveDateStart = request.UnactiveDateStart?.ToUniversalTime(),
                UnactiveDateEnd = request.UnactiveDateEnd?.ToUniversalTime(),
                AssignmentId = request.AssignmentId
            };

            var teamInterviews = this.teamInterviewViewFactory.Load(input);

            foreach (var x in teamInterviews.Items) foreach (var y in x.FeaturedQuestions) y.Question = y.Question.RemoveHtmlTags();

            var response = new TeamInterviewsDataTableResponse
            {
                Draw = request.Draw + 1,
                RecordsTotal = teamInterviews.TotalCount,
                RecordsFiltered = teamInterviews.TotalCount,
                Data = teamInterviews.Items
            };

            return response;
        }


        [HttpPost]
        [Authorize(Roles = "Administrator, Supervisor, Headquarter")]
        public List<CommentedStatusHistoryView> ChangeStateHistory(ChangeStateHistoryViewModel data)
        {
            var interviewSummary = this.changeStatusFactory.GetFilteredStatuses(data.InterviewId);

            return interviewSummary;
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public InterviewSummaryForMapPointView InterviewSummaryForMapPoint(InterviewSummaryForMapPointViewModel data)
        {
            return data == null ? null : GetInterviewSummaryForMapPointView(data.InterviewId);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        public InterviewSummaryForMapPointView[] InterviewSummaryForMapPoints(InterviewSummaryForMapPointsViewModel data)
        {
            return data?.InterviewIds?.Select(GetInterviewSummaryForMapPointView).ToArray();
        }

        private InterviewSummaryForMapPointView GetInterviewSummaryForMapPointView(Guid interviewId)
        {
            var interviewSummaryView = this.interviewSummaryViewFactory.Load(interviewId);
            if (interviewSummaryView == null)
                return null;

            var interviewSummaryForMapPointView = new InterviewSummaryForMapPointView
            {
                InterviewerName = interviewSummaryView.ResponsibleName,
                SupervisorName = interviewSummaryView.TeamLeadName,
                InterviewKey = interviewSummaryView.Key,
                AssignmentId = interviewSummaryView.AssignmentId,
                LastStatus = interviewSummaryView.Status.ToLocalizeString(),
                LastUpdatedDate = AnswerUtils.AnswerToString(interviewSummaryView.UpdateDate),
                InterviewId = interviewSummaryView.InterviewId
            };
            return interviewSummaryForMapPointView;
        }
    }
}
