using System.Linq;
using Raven.Client.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus
{
    public class ChangeStatusFactory : IViewFactory<ChangeStatusInputModel, ChangeStatusView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewStatusHistory> interviews;

        public ChangeStatusFactory(IQueryableReadSideRepositoryReader<InterviewStatusHistory> interviews)
        {
            this.interviews = interviews;
        }

        public ChangeStatusView Load(ChangeStatusInputModel input)
        {
            var interviewSummary = this.interviews.Query(_ => _.FirstOrDefault(x => x.InterviewId == input.InterviewId.FormatGuid()));
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
