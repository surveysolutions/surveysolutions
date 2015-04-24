using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus
{
    public class ChangeStatusFactory : IViewFactory<ChangeStatusInputModel, ChangeStatusView>
    {
        private readonly IReadSideKeyValueStorage<InterviewStatusHistory> interviews;

        public ChangeStatusFactory(IReadSideKeyValueStorage<InterviewStatusHistory> interviews)
        {
            this.interviews = interviews;
        }

        public ChangeStatusView Load(ChangeStatusInputModel input)
        {
            var interviewSummary = this.interviews.GetById(input.InterviewId);
            if (interviewSummary == null)
                return null;

            return new ChangeStatusView {
                    StatusHistory = interviewSummary.StatusChangeHistory
                                                    .Select(x => new CommentedStatusHistroyView {
                                                        Comment = x.Comment,
                                                        Date = x.Date,
                                                        Status = x.Status,
                                                        Responsible = x.Responsible
                                                    }).ToList()
                };
        }
    }
}
