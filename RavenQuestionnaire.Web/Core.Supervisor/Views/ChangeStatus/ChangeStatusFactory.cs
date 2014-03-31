using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Interview;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.ChangeStatus
{
    public class ChangeStatusFactory : IViewFactory<ChangeStatusInputModel, ChangeStatusView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;

        public ChangeStatusFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.interviews = interviews;
        }

        public ChangeStatusView Load(ChangeStatusInputModel input)
        {
            var interviewSummary = interviews.GetById(input.InterviewId);
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
                            Status = x.Status
                        }).ToList(),
                    FeaturedQuestions = interviewSummary.AnswersToFeaturedQuestions.Values.Select(a => new InterviewFeaturedQuestion
                    {
                        Id = a.Id,
                        Answer = a.Answer,
                        Question = a.Title
                    }).ToList()
                };
        }
    }
}
