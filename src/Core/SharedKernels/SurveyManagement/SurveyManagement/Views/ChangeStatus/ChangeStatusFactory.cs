using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus
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
                StatusHistory =
                    interviewStatusChangeHistory.InterviewCommentedStatuses.Where(
                        i => i.Status.ConvertToInterviewStatus().HasValue)
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
