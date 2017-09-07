using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus
{
    public interface IChangeStatusFactory
    {
        ChangeStatusView Load(ChangeStatusInputModel input);

        List<CommentedStatusHistroyView> GetFilteredStatuses(Guid interviewId);
    }

    public class ChangeStatusFactory : IChangeStatusFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;

        public ChangeStatusFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.interviews = interviews;
        }

        public List<CommentedStatusHistroyView> GetFilteredStatuses(Guid interviewId)
        {
            var interviewStatusChangeHistory = this.interviews.GetById(interviewId);

            var statuses = interviewStatusChangeHistory?.InterviewCommentedStatuses
                .Where(i => i.Status.ConvertToInterviewStatus().HasValue);

            bool IsAutomaticallyAssignedToSv (InterviewCommentedStatus x) => x.Status == InterviewExportedAction.SupervisorAssigned && x.StatusChangeOriginatorRole == UserRoles.Interviewer;
            bool IsAutomaticallyAssignedToIn(InterviewCommentedStatus x) => x.Status == InterviewExportedAction.InterviewerAssigned && !x.InterviewerId.HasValue;

            var filteredStatuses = statuses?
                .Where(x => !IsAutomaticallyAssignedToSv(x) && !IsAutomaticallyAssignedToIn(x))
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
                Date = commentedStatus.Timestamp,
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
            switch (commentedStatus.Status)
            {
                case InterviewExportedAction.Created:
                    return commentedStatus.StatusChangeOriginatorName;
                case InterviewExportedAction.Completed:
                case InterviewExportedAction.RejectedByHeadquarter:
                case InterviewExportedAction.SupervisorAssigned:
                    return commentedStatus.SupervisorName;
                case InterviewExportedAction.ApprovedBySupervisor:
                case InterviewExportedAction.UnapprovedByHeadquarter:
                    return Strings.AnyHeadquarters;
                case InterviewExportedAction.ApprovedByHeadquarter:
                    return String.Empty;
            }

            return commentedStatus.InterviewerName;
        }

        private string GetAssigneeRole(InterviewCommentedStatus commentedStatus)
        {
            switch (commentedStatus.Status)
            {
                case InterviewExportedAction.Created:
                    return commentedStatus.StatusChangeOriginatorRole.ToString();
                case InterviewExportedAction.Completed:
                case InterviewExportedAction.RejectedByHeadquarter:
                case InterviewExportedAction.SupervisorAssigned:
                    return UserRoles.Supervisor.ToString();
                case InterviewExportedAction.ApprovedBySupervisor:
                case InterviewExportedAction.UnapprovedByHeadquarter:
                    return UserRoles.Headquarter.ToString();
                case InterviewExportedAction.ApprovedByHeadquarter:
                    return String.Empty;
                case InterviewExportedAction.InterviewerAssigned:
                    if (string.IsNullOrWhiteSpace(commentedStatus.InterviewerName))
                        return string.Empty;
                    break;
            }

            return UserRoles.Interviewer.ToString();
        }
    }
}
