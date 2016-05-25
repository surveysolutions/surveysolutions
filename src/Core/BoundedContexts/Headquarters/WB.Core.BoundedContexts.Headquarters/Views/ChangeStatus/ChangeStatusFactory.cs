using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus
{
    public class ChangeStatusFactory : IViewFactory<ChangeStatusInputModel, ChangeStatusView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviews;

        public ChangeStatusFactory(IQueryableReadSideRepositoryReader<InterviewStatuses> interviews)
        {
            this.interviews = interviews;
        }

        public ChangeStatusView Load(ChangeStatusInputModel input)
        {
            var interviewStatusChangeHistory = this.interviews.GetById(input.InterviewId);

            return new ChangeStatusView
            {
                StatusHistory = interviewStatusChangeHistory == null
                    ? new List<CommentedStatusHistroyView>()
                    : interviewStatusChangeHistory.InterviewCommentedStatuses.Where(
                        i => InterviewExportedActionUtils.ConvertToInterviewStatus(i.Status).HasValue)
                        .Select(x => new CommentedStatusHistroyView
                        {
                            Comment = x.Comment,
                            Date = x.Timestamp,
                            Status = x.Status.ConvertToInterviewStatus().Value,
                            Responsible = x.StatusChangeOriginatorName
                        }).ToList()
            };
        }
    }
}
