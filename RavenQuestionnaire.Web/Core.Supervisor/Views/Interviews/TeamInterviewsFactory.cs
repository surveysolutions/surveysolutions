using System;
using System.Linq;
using System.Linq.Expressions;
using Core.Supervisor.Views.Interview;
using Core.Supervisor.Views.Survey;
using Main.Core.Utility;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Core.Supervisor.Views.Interviews
{
    public class TeamInterviewsFactory : IViewFactory<TeamInterviewsInputModel, TeamInterviewsView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;

        public TeamInterviewsFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.interviews = interviews;
        }

        public TeamInterviewsView Load(TeamInterviewsInputModel input)
        {
            Expression<Func<InterviewSummary, bool>> predicate = (i) => !i.IsDeleted;

            if (input.Status.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.Status == input.Status));
            }

            if (input.QuestionnaireId.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.QuestionnaireId == input.QuestionnaireId));
            }

            if (input.QuestionnaireVersion.HasValue)
            {
                predicate = predicate.AndCondition(x => (x.QuestionnaireVersion == input.QuestionnaireVersion));
            }

            predicate = input.ResponsibleId.HasValue
                ? predicate.AndCondition(x => x.ResponsibleId == input.ResponsibleId)
                : predicate.AndCondition(x => x.TeamLeadId != null && x.TeamLeadId == input.ViewerId);
            

            var interviewItems = this.DefineOrderBy(this.interviews.Query(_ => _.Where(predicate)), input)
                            .Skip((input.Page - 1) * input.PageSize)
                            .Take(input.PageSize).ToList();


            return new TeamInterviewsView()
            {
                TotalCount = this.interviews.Query(_ => _.Count(predicate)),
                Items = interviewItems.Select(x => new TeamInterviewsViewItem()
                {
                    FeaturedQuestions = x.AnswersToFeaturedQuestions.Values.Select(a => new InterviewFeaturedQuestion()
                    {
                        Id = a.Id,
                        Answer = a.Answer,
                        Question = a.Title
                    }),
                    InterviewId = x.InterviewId,
                    LastEntryDate = x.UpdateDate.ToShortDateString(),
                    ResponsibleId = x.ResponsibleId,
                    ResponsibleName = x.ResponsibleName,
                    Status = x.Status.ToString(),
                    CanBeReassigned = x.Status == InterviewStatus.Created
                                      || x.Status == InterviewStatus.SupervisorAssigned
                                      || x.Status == InterviewStatus.InterviewerAssigned
                                      || x.Status == InterviewStatus.RejectedBySupervisor
                })
            };   
        }

        private IQueryable<InterviewSummary> DefineOrderBy(IQueryable<InterviewSummary> query, TeamInterviewsInputModel model)
        {
            var orderBy = model.Orders.FirstOrDefault();
            if (orderBy == null)
            {
                return query;
            }
            return query.OrderUsingSortExpression(model.Order).AsQueryable();
        }

    }
}
