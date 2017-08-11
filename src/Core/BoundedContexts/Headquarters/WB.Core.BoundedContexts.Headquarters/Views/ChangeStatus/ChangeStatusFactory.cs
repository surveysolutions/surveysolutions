using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus
{
    public interface IChangeStatusFactory
    {
        ChangeStatusView Load(ChangeStatusInputModel input);
    }

    public class ChangeStatusFactory : IChangeStatusFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviews;

        public ChangeStatusFactory(IQueryableReadSideRepositoryReader<InterviewStatuses> interviews)
        {
            this.interviews = interviews;
        }

        public ChangeStatusView Load(ChangeStatusInputModel input)
        {
            var interviewStatusChangeHistory = this.interviews.GetById(input.InterviewId);

            var commentedStatusHistroyViews = interviewStatusChangeHistory?.InterviewCommentedStatuses
                .Where(i => i.Status.ConvertToInterviewStatus().HasValue)
                .Select(x => new CommentedStatusHistroyView
                {
                    Comment = x.Comment,
                    Date = x.Timestamp.ToLocalTime(),
                    DateHumanized = x.Timestamp.ToLocalTime().FormatDateWithTime(),
                    Status = x.Status.ConvertToInterviewStatus().Value,
                    StatusHumanized = x.Status.ConvertToInterviewStatus().Value.ToLocalizeString(),
                    Responsible = x.StatusChangeOriginatorName,
                    ResponsibleRole = x.StatusChangeOriginatorRole.ToString(),
                    Assignee = GetAssigneeName(x),
                    AssigneeRole = GetAssigneeRole(x)
                }).ToList() ?? new List<CommentedStatusHistroyView>();
            
            return new ChangeStatusView
            {
                StatusHistory = commentedStatusHistroyViews
            };
        }

        private string GetAssigneeName(InterviewCommentedStatus commentedStatus)
        {
            if (commentedStatus.Status == InterviewExportedAction.Created)
                return commentedStatus.StatusChangeOriginatorName;
            if (commentedStatus.Status == InterviewExportedAction.Completed)
                return commentedStatus.SupervisorName;
            return commentedStatus.InterviewerName;
        }

        private string GetAssigneeRole(InterviewCommentedStatus commentedStatus)
        {
            if (commentedStatus.Status == InterviewExportedAction.Created)
                return commentedStatus.StatusChangeOriginatorName;

            if (commentedStatus.Status == InterviewExportedAction.Completed)
                return UserRoles.Supervisor.ToString();
            return UserRoles.Interviewer.ToString();
        }
    }
}
