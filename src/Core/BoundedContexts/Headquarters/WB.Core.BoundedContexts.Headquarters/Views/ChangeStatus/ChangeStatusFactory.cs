using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus
{
    public interface IChangeStatusFactory
    {
        ChangeStatusView Load(ChangeStatusInputModel input);

        List<CommentedStatusHistroyView> GetFilteredStatuses(Guid interviewId);
    }

    public class ChangeStatusFactory : IChangeStatusFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviews;

        public ChangeStatusFactory(IQueryableReadSideRepositoryReader<InterviewStatuses> interviews)
        {
            this.interviews = interviews;
        }

        public List<CommentedStatusHistroyView> GetFilteredStatuses(Guid interviewId)
        {
            var interviewStatusChangeHistory = this.interviews.GetById(interviewId);

            var statuses = interviewStatusChangeHistory?.InterviewCommentedStatuses
                .Where(i => i.Status.ConvertToInterviewStatus().HasValue);

            bool IsAutomaticallyAssignedToSv (InterviewCommentedStatus x) => x.Status == InterviewExportedAction.SupervisorAssigned && x.StatusChangeOriginatorRole == UserRoles.Interviewer;

            var filteredStatuses = statuses?
                .Where(x => !IsAutomaticallyAssignedToSv(x))
                .Select(CreateCommentedStatusHistroyView).ToList() ?? new List<CommentedStatusHistroyView>();

            return filteredStatuses;
        }
            
        public ChangeStatusView Load(ChangeStatusInputModel input)
        {
            var interviewStatusChangeHistory = this.interviews.GetById(input.InterviewId);

            var commentedStatusHistroyViews = interviewStatusChangeHistory?.InterviewCommentedStatuses
                .Where(i => i.Status.ConvertToInterviewStatus().HasValue)
                .Select(CreateCommentedStatusHistroyView).ToList() ?? new List<CommentedStatusHistroyView>();
            
            return new ChangeStatusView
            {
                StatusHistory = commentedStatusHistroyViews
            };
        }

        private CommentedStatusHistroyView CreateCommentedStatusHistroyView(InterviewCommentedStatus commentedStatus)
        {
            return new CommentedStatusHistroyView
            {
                Comment = commentedStatus.Comment,
                Date = commentedStatus.Timestamp.ToLocalTime(),
                DateHumanized = commentedStatus.Timestamp.ToLocalTime().FormatDateWithTime(),
                Status = commentedStatus.Status.ConvertToInterviewStatus().Value,
                StatusHumanized = commentedStatus.Status.ConvertToInterviewStatus().Value.ToLocalizeString(),
                Responsible = commentedStatus.StatusChangeOriginatorName,
                ResponsibleRole = commentedStatus.StatusChangeOriginatorRole.ToString().ToLower(),
                Assignee = GetAssigneeName(commentedStatus),
                AssigneeRole = GetAssigneeRole(commentedStatus).ToLower()
            };
        }

        private string GetAssigneeName(InterviewCommentedStatus commentedStatus)
        {
            if (commentedStatus.Status == InterviewExportedAction.Created)
                return commentedStatus.StatusChangeOriginatorName;

            if (commentedStatus.Status == InterviewExportedAction.Completed || commentedStatus.Status == InterviewExportedAction.RejectedByHeadquarter)
                return commentedStatus.SupervisorName;

            if (commentedStatus.Status == InterviewExportedAction.ApprovedBySupervisor)
                return Strings.AnyHeadquarters;

            if (commentedStatus.Status == InterviewExportedAction.ApprovedByHeadquarter)
                return String.Empty;

            return commentedStatus.InterviewerName;
        }

        private string GetAssigneeRole(InterviewCommentedStatus commentedStatus)
        {
            if (commentedStatus.Status == InterviewExportedAction.Created)
                return commentedStatus.StatusChangeOriginatorRole.ToString();

            if (commentedStatus.Status == InterviewExportedAction.Completed || commentedStatus.Status == InterviewExportedAction.RejectedByHeadquarter)
                return UserRoles.Supervisor.ToString();

            if (commentedStatus.Status == InterviewExportedAction.ApprovedBySupervisor)
                return UserRoles.Headquarter.ToString();

            if (commentedStatus.Status == InterviewExportedAction.ApprovedByHeadquarter)
                return String.Empty;

            return UserRoles.Interviewer.ToString();
        }
    }
}
