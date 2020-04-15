using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize]
    [Route("api/{controller}/{action}")]
    public class InterviewApiController : ControllerBase
    {
        private readonly IAllInterviewsFactory allInterviewsViewFactory;
        private readonly IChangeStatusFactory changeStatusFactory;

        public InterviewApiController(
            IAllInterviewsFactory allInterviewsViewFactory,
            IChangeStatusFactory changeStatusFactory)
        {
            this.allInterviewsViewFactory = allInterviewsViewFactory;
            this.changeStatusFactory = changeStatusFactory;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Supervisor, Headquarter")]
        public List<CommentedStatusHistoryView> ChangeStateHistory([FromBody]ChangeStateHistoryViewModel data)
        {
            var interviewSummary = this.changeStatusFactory.GetFilteredStatuses(data.InterviewId);

            return interviewSummary;
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        public InterviewSummaryForMapPointView InterviewSummaryForMapPoint([FromBody]InterviewSummaryForMapPointViewModel data)
        {
            return data == null ? null : GetInterviewSummaryForMapPointView(data.InterviewId);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor, Interviewer")]
        public InterviewSummaryForMapPointView[] InterviewSummaryForMapPoints([FromBody]InterviewSummaryForMapPointsViewModel data)
        {
            return data?.InterviewIds?.Select(GetInterviewSummaryForMapPointView).ToArray();
        }

        private InterviewSummaryForMapPointView GetInterviewSummaryForMapPointView(Guid interviewId)
        {
            var interviewSummaryView = this.allInterviewsViewFactory.Load(interviewId);
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
