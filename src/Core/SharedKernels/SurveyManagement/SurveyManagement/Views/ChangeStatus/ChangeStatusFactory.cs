using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus
{
    public class ChangeStatusFactory : IViewFactory<ChangeStatusInputModel, ChangeStatusView>
    {
        private readonly IReadSideRepositoryReader<InterviewSummary> interviews;

        public ChangeStatusFactory(IReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.interviews = interviews;
        }

        public ChangeStatusView Load(ChangeStatusInputModel input)
        {
            var interviewSummary = this.interviews.GetById(input.InterviewId);
            if (interviewSummary == null)
                return null;

            return new ChangeStatusView
                {
                    InterviewId = interviewSummary.InterviewId,
                    QuestionnaireId = interviewSummary.QuestionnaireId,
                    QuestionnaireVersion =  interviewSummary.QuestionnaireVersion,
                    QuestionnaireTitle = interviewSummary.QuestionnaireTitle,
                    Status = interviewSummary.Status,
                    StatusHistory = interviewSummary.CommentedStatusesHistory.Select(x => new CommentedStatusHistroyView
                        {
                            Comment = x.Comment,
                            Date = x.Date,
                            Status = x.Status,
                            Responsible = x.Responsible
                        }).ToList(),
                    FeaturedQuestions = interviewSummary.AnswersToFeaturedQuestions.Values.Select(a => new InterviewFeaturedQuestion
                    {
                        Id = a.Id,
                        Answer = a.Answer,
                        Question = a.Title
                    }).ToList(),
                    Responsible = interviewSummary.ResponsibleName
                };
        }
    }
}
