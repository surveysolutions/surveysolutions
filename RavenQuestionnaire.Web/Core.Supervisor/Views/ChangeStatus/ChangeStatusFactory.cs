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
            var interview = interviews.GetById(input.InterviewId);

            return new ChangeStatusView
                {
                    InterviewId = interview.InterviewId,
                    QuestionnaireId = interview.QuestionnaireId,
                    QuestionnaireVersion =  interview.QuestionnaireVersion,
                    QuestionnaireTitle = interview.QuestionnaireTitle,
                    Status = interview.Status,
                    StatusHistory = interview.CommentedStatusesHistory.Select(x => new CommentedStatusHistroyView
                        {
                            Comment = x.Comment,
                            Date = x.Date,
                            Status = x.Status
                        }).ToList(),
                    FeaturedQuestions = interview.AnswersToFeaturedQuestions.Values.Select(a => new InterviewFeaturedQuestion
                    {
                        Id = a.Id,
                        Answer = a.Answer,
                        Question = a.Title
                    }).ToList()
                };
        }
    }
}
